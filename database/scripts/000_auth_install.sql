-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "000_auth_install.sql"

:r .\schema\001_auth_schema.sql
:r .\seeds\001_auth_seed.sql
:r .\stored-procedures\001_auth_procedures.sql
