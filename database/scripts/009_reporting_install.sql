-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "009_reporting_install.sql"

:r .\schema\010_reporting_schema.sql
:r .\seeds\010_reporting_seed.sql
:r .\stored-procedures\010_reporting_procedures.sql
