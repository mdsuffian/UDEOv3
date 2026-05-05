using UDEO.Core;
using UDEO.Core.Models;
using UDEO.Experts;
using UDEO.Experts.BuiltIn;
using UDEO.Pipeline;
using UDEO.Pipeline.Templates;
using UDEO.Store;
using UDEO.Telemetry;
using Xunit;

namespace UDEO.Tests;

public class CoreTests
{
    [Fact]
    public void DecisionTrace_Creation_AssignsValues()
    {
        var trace = new DecisionTrace("test.expert", "Test Expert", "RULE_1", "VALID", 23.5);
        Assert.Equal("test.expert", trace.ExpertId);
        Assert.Equal("RULE_1", trace.RuleFired);
        Assert.Equal("VALID", trace.DecisionCode);
        Assert.Equal(23.5, trace.ExecutionTimeMs);
        Assert.True(trace.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void ExecutionContext_RecordDecision_IncrementsStep()
    {
        var ctx = new ExecutionContext("test-pipeline");
        Assert.Equal(0, ctx.Step);

        ctx.RecordDecision(new DecisionTrace("e1", "E1", "R1", "VALID", 1.0));
        Assert.Equal(1, ctx.Step);
        Assert.Single(ctx.GetTrace());
    }

    [Fact]
    public void ExecutionContext_Concurrent_IsThreadSafe()
    {
        var ctx = new ExecutionContext("concurrent-test");
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var idx = i;
            tasks.Add(Task.Run(() =>
                ctx.RecordDecision(new DecisionTrace($"expert_{idx}", $"E{idx}", $"R{idx}", "VALID", idx))));
        }
        Task.WaitAll(tasks.ToArray());
        Assert.Equal(100, ctx.Step);
        Assert.Equal(100, ctx.GetTrace().Count);
    }
}

public class ExpertTests
{
    [Fact]
    public void ValidationExpert_PositiveNumber_Passes()
    {
        ValidationExpert.Register();
        var ctx = new ExecutionContext("test");
        ctx.Data["amount"] = 500.0;

        var result = ExpertExecutor.Instance.Execute("udeo.validation", ctx,
            new Dictionary<string, object?> { ["Field"] = "amount", ["Schema"] = "positive_number" });

        Assert.True(result.Success);
        Assert.Equal("VALID", result.DecisionCode);
        Assert.Contains("SCHEMA_POSITIVE_NUMBER", result.RuleFired);
    }

    [Fact]
    public void ValidationExpert_NegativeNumber_Fails()
    {
        ValidationExpert.Register();
        var ctx = new ExecutionContext("test");
        ctx.Data["amount"] = -10.0;

        var result = ExpertExecutor.Instance.Execute("udeo.validation", ctx,
            new Dictionary<string, object?> { ["Field"] = "amount", ["Schema"] = "positive_number" });

        Assert.True(result.Success);
        Assert.Equal("INVALID", result.DecisionCode);
    }

    [Fact]
    public void ValidationExpert_CreditScore_ValidRange()
    {
        ValidationExpert.Register();
        var ctx = new ExecutionContext("test");
        ctx.Data["score"] = 720;

        var result = ExpertExecutor.Instance.Execute("udeo.validation", ctx,
            new Dictionary<string, object?> { ["Field"] = "score", ["Schema"] = "credit_score" });

        Assert.Equal("VALID", result.DecisionCode);
    }

    [Fact]
    public void ValidationExpert_CreditScore_OutOfRange()
    {
        ValidationExpert.Register();
        var ctx = new ExecutionContext("test");
        ctx.Data["score"] = 200; // Below 300

        var result = ExpertExecutor.Instance.Execute("udeo.validation", ctx,
            new Dictionary<string, object?> { ["Field"] = "score", ["Schema"] = "credit_score" });

        Assert.Equal("INVALID", result.DecisionCode);
    }

    [Fact]
    public void MathExpert_Dti_CalculatesCorrectly()
    {
        MathExpert.Register();
        var ctx = new ExecutionContext("test");
        ctx.Data["monthly_income"] = 5000.0;
        ctx.Data["monthly_debt"] = 1500.0;

        var result = ExpertExecutor.Instance.Execute("udeo.math", ctx,
            new Dictionary<string, object?> { ["Operation"] = "dti" });

        Assert.True(result.Success);
        Assert.True(ctx.Data.TryGetValue("calculations", out var calcs));
        var calcDict = Assert.IsType<Dictionary<string, object>>(calcs);
        Assert.Equal(30.0, calcDict["dti"]); // 1500/5000*100 = 30%
    }

    [Fact]
    public void RiskExpert_DefaultRules_TriggerOnLowCredit()
    {
        RiskExpert.Register();
        var ctx = new ExecutionContext("test");
        ctx.Data["credit_score"] = 550;

        var result = ExpertExecutor.Instance.Execute("udeo.risk", ctx, new Dictionary<string, object?>());

        Assert.Equal("REJECTED", result.DecisionCode);
        Assert.Contains("credit score", result.RuleFired?.ToLowerInvariant() ?? "");
    }
}

public class PipelineTests
{
    [Fact]
    public void Pipeline_BasicExecution_RunsAllSteps()
    {
        ValidationExpert.Register();
        MathExpert.Register();

        var pipeline = new UdeoPipeline("test-pipeline");
        pipeline.Context.Data["value"] = 42;

        pipeline.AddStep("udeo.validation", new Dictionary<string, object?>
        {
            ["Field"] = "value", ["Schema"] = "positive_number"
        });

        var result = pipeline.Run();
        Assert.True(result.Success);
        Assert.Single(result.Trace);
    }

    [Fact]
    public void LoanApprovalPipeline_CreatesAndRuns()
    {
        ValidationExpert.Register();
        MathExpert.Register();
        RiskExpert.Register();
        HumanReviewExpert.Register();

        var pipeline = LoanApprovalPipeline.Create(75000, 25000, 720, 300000, 0.065, 360, 375000);
        var result = pipeline.Run();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Trace);
    }

    [Fact]
    public void Pipeline_ConditionalStep_SkipsWhenConditionTrue()
    {
        ValidationExpert.Register();
        var pipeline = new UdeoPipeline("cond-test");
        pipeline.Context.Data["should_skip"] = true;

        pipeline.AddConditionalStep("udeo.validation",
            new Dictionary<string, object?> { ["Field"] = "nonexistent" },
            ctx => ctx.Data.TryGetValue("should_skip", out var v) && v is true);

        var result = pipeline.Run();
        Assert.Equal(0, result.Trace.Count);
    }

    [Fact]
    public void Pipeline_FailurePolicyStop_StopsPipeline()
    {
        var pipeline = new UdeoPipeline("fail-test");

        // Use an expert that doesn't exist
        pipeline.AddStep("nonexistent.expert", onFailure: FailurePolicy.Stop);

        var result = pipeline.Run();
        Assert.False(result.Success);
        Assert.Equal("ERROR", result.Decision);
    }

    [Fact]
    public void Pipeline_FailurePolicyContinue_Continues()
    {
        ValidationExpert.Register();
        HumanReviewExpert.Register();

        var pipeline = new UdeoPipeline("continue-test");
        pipeline.Context.Data["field"] = "hello";

        // First step fails gracefully
        pipeline.AddStep("nonexistent.expert", onFailure: FailurePolicy.Continue);
        // Second step succeeds
        pipeline.AddStep("udeo.validation", new Dictionary<string, object?>
        {
            ["Field"] = "field", ["Schema"] = "non_empty_string"
        });

        var result = pipeline.Run();
        Assert.True(result.Success);
        Assert.Equal(2, result.Trace.Count);
    }
}

public class StoreTests
{
    [Fact]
    public void Store_SaveAndLoad_RoundTrips()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"udeo_test_{Guid.NewGuid():N}");
        try
        {
            UdeoStore.Instance.Initialize(tempDir);
            var ctx = new ExecutionContext("store-test");
            ctx.Data["test_key"] = "test_value";
            ctx.RecordDecision(new DecisionTrace("e1", "E1", "R1", "VALID", 1.0));

            UdeoStore.Instance.Save(ctx);

            var loaded = UdeoStore.Instance.Load("store-test");
            Assert.NotNull(loaded);
            Assert.True(loaded!.Data.TryGetValue("test_key", out var val));
            Assert.Equal("test_value", val?.ToString());
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Store_List_ReturnsSavedIds()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"udeo_test_{Guid.NewGuid():N}");
        try
        {
            UdeoStore.Instance.Initialize(tempDir);
            UdeoStore.Instance.Save(new ExecutionContext("list-test-1"));
            UdeoStore.Instance.Save(new ExecutionContext("list-test-2"));

            var list = UdeoStore.Instance.List();
            Assert.Contains("list-test-1", list);
            Assert.Contains("list-test-2", list);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}

public class TelemetryTests
{
    [Fact]
    public void Telemetry_StartEndSpan_TracksDuration()
    {
        UdeoTelemetry.Instance.Reset();
        var id = UdeoTelemetry.Instance.StartSpan("test-span");
        Thread.Sleep(10);
        UdeoTelemetry.Instance.EndSpan("test-span", id);

        var summary = UdeoTelemetry.Instance.GetSummary();
        Assert.True(summary.Spans.ContainsKey("test-span"));
        Assert.Equal(1, summary.Spans["test-span"].Count);
    }

    [Fact]
    public void Telemetry_Counter_Increments()
    {
        UdeoTelemetry.Instance.Reset();
        UdeoTelemetry.Instance.Inc("test-counter", 5);
        UdeoTelemetry.Instance.Inc("test-counter", 3);

        var summary = UdeoTelemetry.Instance.GetSummary();
        Assert.True(summary.Counters.ContainsKey("test-counter"));
        Assert.Equal(8, summary.Counters["test-counter"]);
    }

    [Fact]
    public void Telemetry_Histogram_RecordsValues()
    {
        UdeoTelemetry.Instance.Reset();
        UdeoTelemetry.Instance.Record("test-hist", 10.0);
        UdeoTelemetry.Instance.Record("test-hist", 20.0);

        var summary = UdeoTelemetry.Instance.GetSummary();
        Assert.True(summary.Histograms.ContainsKey("test-hist"));
        Assert.Equal(2, summary.Histograms["test-hist"].Count);
        Assert.Equal(15.0, summary.Histograms["test-hist"].Avg);
    }
}
