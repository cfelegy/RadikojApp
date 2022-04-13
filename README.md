# RadikojApp
Source code for the Antro Radikoj website.

This is a fully custom CMS featuring first-class support for the five United Nations languages, automatic translation powered by Azure Translation Service (easily audited and altered by site administrators), and a custom interface for developing and deploying surveys.

## Development Environment
Developed using .NET 6 and PostgreSQL. Some application settings will need to be set; see the placeholder values in `radikoj/appsettings.json`. Most importantly, the environment needs:

- PostgreSQL
- Azure Translation (API key)
- SendGrid (API key)

SendGrid is only used for user login. If the environment is trustworthy, a free plan (100 mails/day) will more than suffice.