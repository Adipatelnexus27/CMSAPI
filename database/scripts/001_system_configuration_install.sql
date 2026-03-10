-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "001_system_configuration_install.sql"

:r .\schema\002_system_configuration_schema.sql
:r .\seeds\002_system_configuration_seed.sql
:r .\stored-procedures\002_system_configuration_procedures.sql
