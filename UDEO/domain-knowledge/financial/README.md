# Financial Domain Knowledge

This folder contains domain-specific rules, regulations, and validation logic for the financial domain.

## Structure

```
financial/
├── regulations/            # Financial regulations and compliance rules
│   ├── lending-rules.json         # Rules related to lending practices
│   ├── compliance-rules.json      # General compliance requirements
│   └── risk-assessment-rules.json # Risk assessment guidelines
└── validation/             # Financial data validation logic
    ├── loan-validation.json       # Validation for loan applications
    └── credit-validation.json     # Validation for credit assessments
```

## Usage

### Regulations

The `regulations/` folder contains rules and regulations that govern financial processes:

- **lending-rules.json**: Rules for loan approval processes, including DTI calculations, LTV ratios, etc.
- **compliance-rules.json**: General compliance requirements such as KYC, AML, and other regulatory requirements
- **risk-assessment-rules.json**: Guidelines for assessing and managing financial risk

### Validation

The `validation/` folder contains validation logic for financial data:

- **loan-validation.json**: Validation rules for loan application data
- **credit-validation.json**: Validation rules for credit assessment data

## Integration with UDEO

These financial domain knowledge files are used by:

1. **Math Expert Plugin**: For financial calculations (DTI, LTV, etc.)
2. **Validation Expert Plugins**: For validating financial data
3. **Custom Financial Experts**: For domain-specific financial analysis

## Compliance

All rules and validation logic in this folder should comply with:

- Relevant financial regulations in your jurisdiction
- Industry best practices
- Organizational policies and procedures

## Maintenance

Regular updates are required to:

- Reflect changes in financial regulations
- Incorporate new compliance requirements
- Update validation logic based on feedback

## Adding New Rules

When adding new financial rules:

1. Create or update the appropriate JSON file in `regulations/`
2. Validate against the domain-rule-schema.json
3. Update this README if adding new categories
4. Test with relevant expert plugins

## Adding New Validation Logic

When adding new validation logic:

1. Create or update the appropriate JSON file in `validation/`
2. Validate against the validation-logic-schema.json
3. Include comprehensive test cases
4. Update this README if adding new validation types