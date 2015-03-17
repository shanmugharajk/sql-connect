
-- SP TO TEST TABLE VALUED PARAMETER

use OrmLite
go
alter procedure spBulkInsert
	@DepartmentDetails DepartmentType readonly
as begin
   
   select * from Department;

   insert into  
	Department(
		 DepartmentId, 
		 Name 
	)
    select 
		DepartmentId, 
		Name
    from   @DepartmentDetails;

	return @@ROWCOUNT

end	 

go