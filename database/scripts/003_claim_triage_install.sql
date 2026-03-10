-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "003_claim_triage_install.sql"

:r .\schema\004_claim_triage_schema.sql
:r .\seeds\004_claim_triage_seed.sql
:r .\stored-procedures\004_claim_triage_procedures.sql
