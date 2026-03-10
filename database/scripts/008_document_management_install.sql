-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "008_document_management_install.sql"

:r .\schema\009_document_management_schema.sql
:r .\seeds\009_document_management_seed.sql
:r .\stored-procedures\009_document_management_procedures.sql
