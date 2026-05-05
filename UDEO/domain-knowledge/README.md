# UDEO Domain Knowledge Repository

This repository contains domain-specific rules, validation logic, and knowledge bases used by UDEO experts to perform their functions across different domains.

## Purpose

The Domain Knowledge Repository serves as a centralized location for storing and managing domain-specific information that UDEO experts can reference when performing tasks. This includes:

- Domain-specific rules and regulations
- Validation logic for domain data
- Best practices and guidelines
- Domain-specific terminology and concepts

## Structure

```
domain-knowledge/
├── schemas/                     # JSON schemas for validating domain knowledge files
├── financial/                   # Financial domain knowledge
│   ├── regulations/            # Financial regulations and compliance rules
│   └── validation/             # Financial data validation logic
├── medical/                    # Medical domain knowledge
│   ├── guidelines/             # Medical guidelines and protocols
│   └── validation/             # Medical data validation logic
├── programming/                # Programming domain knowledge
│   ├── best-practices/         # Coding standards and design patterns
│   ├── validation/             # Code validation rules
│   └── language-specific/      # Language-specific rules (C#, PowerShell, Python)
└── templates/                  # Template files for creating new domain knowledge
```

## Usage

### For Expert Developers

When creating expert plugins that require domain-specific knowledge:

1. Reference the appropriate domain knowledge files in your expert implementation
2. Use the schemas in the `schemas/` directory to validate your domain knowledge files
3. Follow the established patterns when adding new domain knowledge

### For Domain Experts

When contributing domain knowledge:

1. Use the templates in the `templates/` directory as a starting point
2. Validate your files against the appropriate schemas
3. Follow the naming conventions established in each domain folder

### For System Administrators

When configuring UDEO to use domain knowledge:

1. Update the UDEO configuration to point to the appropriate domain knowledge files
2. Ensure the domain knowledge files are accessible to the UDEO system
3. Monitor for updates to domain knowledge and apply as needed

## Integration with UDEO

The domain knowledge repository integrates with UDEO through:

1. **Universal Validation Expert**: Uses validation logic from this repository
2. **Domain-Specific Experts**: Reference rules and guidelines from their respective domains
3. **Expert Orchestration**: Uses domain knowledge to select appropriate experts for tasks

## Contributing

When adding new domain knowledge:

1. Create new files in the appropriate domain folder
2. Validate against the schemas in the `schemas/` directory
3. Update documentation as needed
4. Test with relevant expert plugins

## Maintenance

- Regularly review and update domain knowledge to ensure accuracy
- Version control changes to track evolution of domain knowledge
- Validate changes against schemas to maintain consistency
- Test changes with relevant expert plugins

## Supported Domains

### Financial
- Lending regulations and compliance
- Risk assessment rules
- Financial data validation

### Medical
- Clinical guidelines and protocols
- Treatment guidelines
- Patient data validation

### Programming
- Coding standards and best practices
- Design patterns
- Security practices
- Code validation rules
- Language-specific rules (C#, PowerShell, Python)

## Adding New Domains

To add a new domain:

1. Create a new folder in the `domain-knowledge/` directory
2. Create subfolders for guidelines/regulations and validation logic
3. Add domain-specific knowledge files
4. Create a README.md explaining the domain structure
5. Update this main README.md to include the new domain

## Schema Validation

All domain knowledge files should validate against the schemas in the `schemas/` directory:

- `domain-rule-schema.json`: For domain rules and regulations
- `validation-logic-schema.json`: For validation logic files

This ensures consistency and interoperability across the system.