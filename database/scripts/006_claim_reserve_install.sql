-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "006_claim_reserve_install.sql"

:r .\schema\007_claim_reserve_schema.sql
:r .\seeds\007_claim_reserve_seed.sql
:r .\stored-procedures\007_claim_reserve_procedures.sql
