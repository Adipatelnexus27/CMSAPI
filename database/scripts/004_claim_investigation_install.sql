-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "004_claim_investigation_install.sql"

:r .\schema\005_claim_investigation_schema.sql
:r .\seeds\005_claim_investigation_seed.sql
:r .\stored-procedures\005_claim_investigation_procedures.sql
