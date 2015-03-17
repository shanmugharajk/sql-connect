
-- TABLE TYPE DEPARTMENT
use OrmLite
go

create type DepartmentType as table
(
    DepartmentId       int,
    Name		       varchar(50)
)

go