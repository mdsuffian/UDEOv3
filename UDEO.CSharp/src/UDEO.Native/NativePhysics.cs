using System.Runtime.InteropServices;

namespace UDEO.Native;

/// <summary>
/// P/Invoke interop layer for the UDEO Native C++ physics engine.
/// </summary>
public static class NativePhysics
{
    private const string DllName = "UDEO.Native";

    static NativePhysics()
    {
        // Attempt to load the native library
        try
        {
            NativeLibrary.Load(DllName);
        }
        catch
        {
            // Native library optional — fallback to managed code
        }
    }

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec2D
    {
        public double X;
        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AABB
    {
        public double Left;
        public double Top;
        public double Width;
        public double Height;

        public AABB(double left, double top, double width, double height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CollisionResult
    {
        public int Collided;
        public double NormalX;
        public double NormalY;
        public double Penetration;
        public double ResolvedX;
        public double ResolvedY;

        public bool HasCollision => Collided != 0;
    }

    #endregion

    #region Native Methods

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern CollisionResult udeo_aabb_collision(AABB a, AABB b);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern double udeo_apply_gravity(double vy, double gravity, double maxFallSpeed, double dt);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern double udeo_resolve_axis_collision(double position, double velocity, double minBound, double maxBound);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int udeo_grid_collision(
        byte[] grid, int gridWidth, int gridHeight,
        int checkLeft, int checkTop, int checkWidth, int checkHeight);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern CollisionResult udeo_grid_raycast(
        byte[] grid, int gridWidth, int gridHeight,
        double startX, double startY, double dirX, double dirY, double maxDistance);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern double udeo_mortgage_payment(double principal, double annualRate, int months);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern double udeo_dti_ratio(double monthlyDebt, double monthlyIncome);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern double udeo_ltv_ratio(double loanAmount, double propertyValue);

    #endregion

    #region Managed Wrappers (with fallback)

    private static bool _nativeAvailable;

    public static bool IsNativeAvailable
    {
        get
        {
            try
            {
                if (!_nativeAvailable)
                {
                    NativeLibrary.Load(DllName);
                    _nativeAvailable = true;
                }
            }
            catch { }
            return _nativeAvailable;
        }
    }

    /// <summary>
    /// Tests AABB collision. Falls back to managed if native unavailable.
    /// </summary>
    public static CollisionResult TestAabbCollision(AABB a, AABB b)
    {
        if (IsNativeAvailable)
        {
            try { return udeo_aabb_collision(a, b); }
            catch { _nativeAvailable = false; }
        }
        return TestAabbCollisionManaged(a, b);
    }

    /// <summary>
    /// Applies gravity. Falls back to managed.
    /// </summary>
    public static double ApplyGravity(double vy, double gravity, double maxFallSpeed, double dt = 1.0)
    {
        if (IsNativeAvailable)
        {
            try { return udeo_apply_gravity(vy, gravity, maxFallSpeed, dt); }
            catch { _nativeAvailable = false; }
        }
        return Math.Min(vy + gravity * dt, maxFallSpeed);
    }

    /// <summary>
    /// Grid collision check. Falls back to managed.
    /// </summary>
    public static bool CheckGridCollision(
        byte[] grid, int gridWidth, int gridHeight,
        int left, int top, int width, int height)
    {
        if (IsNativeAvailable)
        {
            try { return udeo_grid_collision(grid, gridWidth, gridHeight, left, top, width, height) != 0; }
            catch { _nativeAvailable = false; }
        }
        return CheckGridCollisionManaged(grid, gridWidth, gridHeight, left, top, width, height);
    }

    /// <summary>
    /// Fast mortgage payment calculation.
    /// </summary>
    public static double ComputeMortgagePayment(double principal, double annualRate, int months)
    {
        if (IsNativeAvailable)
        {
            try { return udeo_mortgage_payment(principal, annualRate, months); }
            catch { _nativeAvailable = false; }
        }
        return ComputeMortgagePaymentManaged(principal, annualRate, months);
    }

    /// <summary>
    /// Fast DTI ratio calculation.
    /// </summary>
    public static double ComputeDtiRatio(double monthlyDebt, double monthlyIncome)
    {
        if (IsNativeAvailable)
        {
            try { return udeo_dti_ratio(monthlyDebt, monthlyIncome); }
            catch { _nativeAvailable = false; }
        }
        return monthlyIncome > 0 ? (monthlyDebt / monthlyIncome) * 100.0 : 0.0;
    }

    /// <summary>
    /// Fast LTV ratio calculation.
    /// </summary>
    public static double ComputeLtvRatio(double loanAmount, double propertyValue)
    {
        if (IsNativeAvailable)
        {
            try { return udeo_ltv_ratio(loanAmount, propertyValue); }
            catch { _nativeAvailable = false; }
        }
        return propertyValue > 0 ? (loanAmount / propertyValue) * 100.0 : 0.0;
    }

    #endregion

    #region Managed Fallback Implementations

    private static CollisionResult TestAabbCollisionManaged(AABB a, AABB b)
    {
        var result = new CollisionResult();
        double aRight = a.Left + a.Width;
        double aBottom = a.Top + a.Height;
        double bRight = b.Left + b.Width;
        double bBottom = b.Top + b.Height;

        if (a.Left >= bRight || aRight <= b.Left || a.Top >= bBottom || aBottom <= b.Top)
            return result;

        result.Collided = 1;

        double overlapLeft = aRight - b.Left;
        double overlapRight = bRight - a.Left;
        double overlapTop = aBottom - b.Top;
        double overlapBottom = bBottom - a.Top;

        double minPenX = Math.Min(overlapLeft, overlapRight);
        double minPenY = Math.Min(overlapTop, overlapBottom);

        if (minPenX < minPenY)
        {
            result.Penetration = minPenX;
            if (overlapLeft < overlapRight)
            {
                result.NormalX = -1;
                result.ResolvedX = a.Left - minPenX;
            }
            else
            {
                result.NormalX = 1;
                result.ResolvedX = a.Left + minPenX;
            }
            result.ResolvedY = a.Top;
        }
        else
        {
            result.Penetration = minPenY;
            if (overlapTop < overlapBottom)
            {
                result.NormalY = -1;
                result.ResolvedY = a.Top - minPenY;
            }
            else
            {
                result.NormalY = 1;
                result.ResolvedY = a.Top + minPenY;
            }
            result.ResolvedX = a.Left;
        }

        return result;
    }

    private static bool CheckGridCollisionManaged(
        byte[] grid, int gridWidth, int gridHeight,
        int left, int top, int width, int height)
    {
        int right = left + width;
        int bottom = top + height;

        left = Math.Max(0, left);
        top = Math.Max(0, top);
        right = Math.Min(gridWidth, right);
        bottom = Math.Min(gridHeight, bottom);

        for (int row = top; row < bottom; row++)
            for (int col = left; col < right; col++)
                if (grid[row * gridWidth + col] != 0)
                    return true;
        return false;
    }

    private static double ComputeMortgagePaymentManaged(double principal, double annualRate, int months)
    {
        if (months <= 0 || principal <= 0) return 0;
        double monthlyRate = annualRate / 12.0;
        if (Math.Abs(monthlyRate) < 1e-12) return principal / months;
        double factor = Math.Pow(1 + monthlyRate, months);
        return principal * (monthlyRate * factor) / (factor - 1);
    }

    #endregion
}
