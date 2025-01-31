# Introduction 
 This is a test project used to show case issues in code.

## Inline code issues.

The format I propose is this:
``` 
   /*
    * Issue: N+1 query problem
    * Should: Use proper eager loading
    * Should: Implement pagination
    * Should: Add caching strategy
    */
   var response = new EmployeesByDepartmentResponse();
```

One can see the issues when they open the source files as comments either:

  - at the top of the file or 
  - inline the actual place where the issue occurs

If you need to see how to fix an issue look into the   [implementation](_reviewed-code/README.md#Implementations) (see comment below) md files.

Click here for an example to see the [EmployeeController.cs](src/FehlerhaftPayroll/Controllers/EmployeeController.cs) with review comments.

## Complete Review Documentation

- We have a [folder _reviewed-code](_reviewed-code/README.md). 

This folder is documentation folder that contains a series of review/implementation md files.
Those have a complete and thorough review and fixes or suggestions on how to make the code better. 

Just click to go to the root readme.md and then navigate back and forth to the different sections.
Each file that you open should have a link back to the root README.md from the reviewed-code folder.

tomorrow we will be better :) 
