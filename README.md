# PROG6212POEPART2-CMCSWebApp

Tshudufhadzo Themba 

ST10461617 

Programming 2B 

POE PART 2: Functional Implementation and UnitTesting 

 

Table of Contents 

​​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​ 

​​ 

 

 

1. Documentation: Functional Implementation 

1.1. Project Overview and Functional Design 

The Contract Monthly Claim System (CMCS) has successfully transitioned from a preliminary prototype (Part 1) to a fully functional web application developed using the ASP.NET Core MVC framework (Microsoft, 2025a). The continued selection of the Model-View-Controller (MVC) architectural pattern remains foundational, as it promotes a robust separation of concerns, thereby enhancing the system's maintainability and scalability (Freeman and Sandell, 2018). 

The primary objective of CMCS is now fully realized: to automate and streamline the monthly claim submission and approval process for contract lecturers. The implementation phase (Part 2) focused on establishing the core business logic, data persistence, and role-based security necessary for real-world operations. 

Key Implementation Strategies for Part 2: 

Data Persistence: Entity Framework Core (EF Core) was integrated as the Object-Relational Mapper (ORM) to manage the interaction between the application and the relational database, enabling all required CRUD (Create, Read, Update, Delete) operations for claim and document data (Microsoft, 2025b). 

Role-Based Security: The application uses ASP.NET Identity to manage user authentication and employs strict Role-Based Authorization (RBA). Access to functional areas, such as approval buttons and specific dashboards, is strictly governed by the user’s assigned role (Lecturer, Programme Coordinator, or Academic Manager). 

Business Logic: Server-side logic within the ClaimsController now calculates the total claim amount (HoursWorked * HourlyRate) upon submission and manages the crucial status update lifecycle (e.g., PendingReview to VerifiedByCoordinator). 

1.2. Database Structure and Data Persistence 

The database structure defined in Part 1 has been instantiated using EF Core migrations. This structure ensures data integrity and the necessary relationships to support the claim lifecycle. 

Claim Entity: This entity is central, maintaining essential details like LecturerName, ClaimPeriod, and the critical Status property (an enumerated type). The Status property is the primary mechanism for controlling the claim's visibility and progression through the approval stages. 

Document Entity: This entity maintains a crucial one-to-many relationship with the Claim. It stores metadata such as the OriginalFileName and the secure SupportingDocumentPath, confirming that the associated documentation is securely tracked and linked to the correct claim record. All data manipulation is handled exclusively through asynchronous methods provided by EF Core to maintain application responsiveness. 

1.3. GUI/UI Layout Implementation and Functionality 

The user interface, built with Bootstrap, is now entirely functional, providing specific workflows for each user role. The focus shifted from static display to dynamic interaction, state representation, and secure command execution. 

Claim Submission and File Upload (Lecturer) 

The Lecturer view is implemented as a secure form, enabling a POST request to the ClaimsController. Crucial implementation features include: 

Secure File Handling: The form is configured with enctype="multipart/form-data" to allow the submission of supporting documents. Server-side code handles the IFormFile stream, restricting file types (e.g., to PDF/DOCX) and saving the file to a controlled, secure path on the file system. 

Data Mapping: Model binding automatically maps form inputs for HoursWorked and HourlyRate to the Claim object, which is then persisted to the database with the initial status set to PendingReview. 

Review and Approval Workflows (Coordinator and Manager) 

The dashboards for the Programme Coordinator and Academic Manager are implemented as dynamic tables that only display claims relevant to their current approval step. 

Action Forms: Approval and rejection actions are implemented using dedicated HTML <form> elements with method="post". This is a critical security measure, replacing simple links and ensuring that state-changing actions are protected by Anti-Forgery Tokens (Microsoft, 2025a). 

Coordinator Actions: Only enabled for claims with Status == ClaimStatus.PendingReview. Actions update the status to either VerifiedByCoordinator or Rejected. 

Manager Actions: Only enabled for claims with Status == ClaimStatus.VerifiedByCoordinator. Actions update the status to either ApprovedByManager or Rejected. 

Claim Details and Status Tracking 

The ViewDetails.cshtml page serves as the single source of truth for a claim. The status is represented dynamically using Razor C# logic: 

A C# @switch statement within the view inspects the Model.Status enum. 

The system then applies corresponding Bootstrap classes and icons (bg-warning, bg-success, bi-check-circle-fill, etc.) to visually and instantly inform the user of the claim’s position in the processing lifecycle. 

 

Unit Test Documentation: Claim Management System 

This document outlines the purpose and functionality of the 17 unit tests defined in ClaimServiceTests.cs and ClaimsControllerTests.cs. These tests ensure that the core business logic (ClaimService) and the user interaction layer (ClaimsController) behave correctly under various scenarios, including valid input, boundary conditions, and user role-specific actions. 

I. ClaimService Tests (8 Total) 

These tests validate the business logic and data manipulation within the ClaimService, focusing on claim creation, retrieval, status updates, and data validation. 

 

Test Method Name 

Purpose 

Key Assertions 

1 

CreateClaimAsync_WithValidData_ReturnsClaimId 

Ensures a claim is successfully created and returns a unique ID when all inputs (claim data and a valid file) are correct. 

Returns a Claim ID greater than 0. Sets the initial claim status to PendingReview. 

2 

CreateClaimAsync_WithNullDocument_ThrowsArgumentException 

Validates that claim creation fails if the required supporting document is missing. 

Throws an ArgumentException with a message indicating the document is required. 

3 

CreateClaimAsync_WithOversizedFile_ThrowsArgumentException 

Checks the file size restriction (5MB limit) on the supporting document upload. 

Throws an ArgumentException with a message indicating the file size limit was exceeded. 

4 

CreateClaimAsync_WithInvalidFileType_ThrowsArgumentException 

Verifies that the service rejects files that are not of the permitted types (e.g., non-PDF, non-DOCX). 

Throws an ArgumentException with a message indicating an invalid file type. 

5 

GetClaimByIdAsync_WithExistingId_ReturnsClaim 

Confirms that a previously created claim can be retrieved correctly using its ID. 

Returns a non-null Claim object, and the returned object's ID matches the requested ID. 

6 

UpdateClaimStatusAsync_WithValidId_UpdatesStatus 

Validates the core status update mechanism by changing a claim's status. 

Retrieves the claim after the update and asserts its status is the newly set value (VerifiedByCoordinator). 

7 

GetPendingClaimsAsync_ForCoordinator_ReturnsOnlyPendingClaims 

Ensures that the Coordinator dashboard data retrieval only pulls claims with the required PendingReview status. 

Returns a list with one or more claims, and all claims in the list have the status PendingReview. 

8 

GetManagerDashboardDataAsync_ReturnsCorrectCounts 

Verifies that the manager dashboard can correctly aggregate total claim counts (simulating a database summary). 

Returns a non-null dashboard object, and the TotalClaimsCount is greater than 0. 

II. ClaimsController Tests (9 Total) 

These tests validate the controller's actions, ensuring correct routing, view rendering, user authentication checks, and handling of user-submitted forms and action requests. 

 

Test Method Name 

Purpose 

Key Assertions 

1 

Index_ReturnsViewWithClaims 

Checks that the Lecturer's index page loads correctly and passes the list of their submitted claims to the view. 

Returns a ViewResult, and the model is a list containing the user's claims. 

2 

Create_Get_ReturnsView 

Ensures the HTTP GET request for the claim creation page successfully returns the expected view. 

Returns a ViewResult with the explicit view name ~/Views/Claim/Create.cshtml. 

3 

Create_Post_WithValidModel_RedirectsToIndex 

Verifies the successful submission of a new claim, ensuring the controller calls the service and redirects the user. 

Returns a RedirectToActionResult to the Index action, and the CreateClaimAsync method is called exactly once with the correct LecturerId. 

4 

ViewDetails_WithValidId_ReturnsView 

Ensures that a user can successfully view the details of a claim by ID. 

Returns a ViewResult with the explicit view name ~/Views/Claim/ViewDetails.cshtml and a non-null Claim model. 

5 

ViewDetails_WithInvalidId_RedirectsToHome 

Tests the failure case for viewing details, ensuring the user is redirected when a claim ID is not found. 

Returns a RedirectToActionResult to the Home/Index page and sets an ErrorMessage in TempData. 

6 

Verify_WithValidId_UpdatesStatusAndRedirects 

Checks the Coordinator's action to verify a claim. 

Calls UpdateClaimStatusAsync with the status VerifiedByCoordinator, redirects to Home/CoordinatorDashboard, and sets a SuccessMessage. 

7 

Approve_WithValidId_UpdatesStatusAndRedirects 

Checks the Manager's action to approve a claim. 

Calls UpdateClaimStatusAsync with the status ApprovedByManager, redirects to Home/ManagerDashboard, and sets a SuccessMessage. 

8 

Reject_AsCoordinator_UpdatesStatusAndRedirects 

Tests the Coordinator's rejection action. 

Calls UpdateClaimStatusAsync with the status Rejected, redirects to Home/CoordinatorDashboard, and sets a SuccessMessage. 

9 

Reject_AsManager_UpdatesStatusAndRedirects 

Tests the Manager's rejection action, ensuring correct redirection for the Manager role. 

Calls UpdateClaimStatusAsync with the status Rejected, redirects to Home/ManagerDashboard, and sets a SuccessMessage. 

 

1.4. Assumptions, Constraints, and Testing Scope 

The following assumptions and constraints governed the implementation phase of the project: 

Assumptions 

Pre-existing Roles: It is assumed that the ASP.NET Identity system has been properly configured and that all users have been correctly assigned one of the three required roles (Lecturer, Coordinator, or Manager). 

Valid Data: It is assumed that the input fields for hourly rate and hours worked will receive valid numerical data from the client, though server-side validation is implemented to prevent injection attacks and ensure data type integrity. 

Constraints 

File Handling Scope: The document upload functionality is limited to saving the file to a local server path and storing the file path and name in the database. The scope does not include integration with external cloud storage services (e.g., Azure Blob Storage or Amazon S3). 

Audit Logging: Comprehensive, low-level audit logging for every single field change is outside the current scope; logging is limited to high-level claim status changes (e.g., Approved, Rejected, Verified). 

Testing Focus: Unit testing efforts are strictly constrained to the core business logic, including the total claim amount calculation function and the state transition logic within the ClaimsController (e.g., ensuring a Coordinator cannot approve a claim that has not been submitted). 

2. UML Class Diagram for Databases 

The UML Class Diagram models the core entities and their relationships, which were successfully mapped to the database schema using Entity Framework Core. 

The diagram illustrates the structural blueprint for the application's persistent data layer. 

2.1. Key Relationships: 

User to Claim (1:M): A single Lecturer (User) can submit multiple claims. The LecturerId foreign key within the Claim entity enforces this one-to-many relationship, ensuring every claim is traceable to its originator. 

Claim to Document (1:M): Each claim can be supported by multiple documents (e.g., timesheets, receipts). The ClaimId foreign key in the Document entity ensures that all supporting evidence is linked directly to the parent claim. 

3. Project Plan (Historical) 

This section contains the original project plan used to guide the development process through both Part 1 and Part 2. 

Phase 

Task ID 

Task Description 

Dependencies 

Estimated Week 

Phase 1: Planning & Prototyping 

1.1. 

Finalise the Project Scope and Requirements 

None 

Week 1 

 

1.2. 

Set up the ASP.NET Core MVC Project Structure 

Task 1.1 

Week 1 

 

1.3. 

Design the Database Schema (UML Class Diagram) 

Task 1.2 

Week 2 

 

1.4. 

Implement ASP.NET Identity and Role-Based Setup 

Task 1.2 

Week 2 

 

1.5. 

Develop the Non-Functional Prototyping Views (GUI/UI) 

Task 1.4 

Week 3 

Phase 2: Functional Implementation 

2.1. 

Implement Entity Framework Core and Database Migrations 

Task 1.3 

Week 4 

 

2.2. 

Implement the Claim Submission and File Upload Functionality (Lecturer) 

Task 2.1 

Week 4 

 

2.3. 

Implement Coordinator Verification and Rejection Actions 

Task 2.1, 2.2 

Week 5 

 

2.4. 

Implement Manager Approval and Rejection Actions 

Task 2.3 

Week 6 

 

2.5. 

Implement Unit Tests for Business Logic (Calculation & Status) 

Task 2.2, 2.3 

Week 6 

3. Finalization & Submission 

3.1. 

Write the detailed Project Documentation and Reporting 

All tasks from Phase 2 

Week 7 

 

3.2. 

Review and refine all code and documentation 

All tasks from Phase 2 

Week 7 

 

3.3. 

Commit changes to the GitHub repository 

Ongoing throughout all phases 

Weeks 1-8 

 

3.4. 

Prepare and submit the final report in Microsoft Word format 

All tasks from Phase 2 

Week 8 

 

Total Estimated Duration 

 

 

2 Months 

 

 

GITHUB Repo link: 

https://github.com/ThembaTshudufhadzo/PROG6212POEPART2-CMCSWebApp.git 

 

YouTube Video Link: 

https://youtu.be/GKdE6K-KUks 



 

 

 

 

 

4. Referencing List  

Books and Journals 

Freeman, A. and Sandell, J. (2018). Pro ASP.NET Core MVC 2. 7th ed. Berkeley, CA: Apress. 

Websites and Online Documentation 

GitHub. (2025). Git and GitHub Documentation. Available at: https://docs.github.com/en/ (Accessed: 16 September 2025). 

Mermaid. (2025). Mermaid Documentation. Available at: https://mermaid.js.org/documentation/  (Accessed: 16 September 2025). 

Microsoft. (2025a). Introduction to ASP.NET Core MVC. Available at: https://docs.microsoft.com/en-us/aspnet/core/mvc/overview  (Accessed: 15 October 2025). 

Microsoft. (2025b). Entity Framework Core Documentation. Available at: https://docs.microsoft.com/en-us/ef/core/  (Accessed: 15 O
