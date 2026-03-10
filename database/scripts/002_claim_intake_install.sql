-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "002_claim_intake_install.sql"

:r .\schema\003_claim_intake_schema.sql
:r .\seeds\003_claim_intake_seed.sql
:r .\stored-procedures\003_claim_intake_procedures.sql
