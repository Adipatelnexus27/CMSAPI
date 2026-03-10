-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "007_claim_settlement_install.sql"

:r .\schema\008_claim_settlement_schema.sql
:r .\seeds\008_claim_settlement_seed.sql
:r .\stored-procedures\008_claim_settlement_procedures.sql
