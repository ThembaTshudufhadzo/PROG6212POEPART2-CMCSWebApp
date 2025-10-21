PROG6212 POE PART 2: Contract Monthly Claim System (CMCS)
Functional Implementation and Unit Testing Documentation
Detail
Value
Student Name: Themba Tshudufhadzo
Student Number :ST10461617
Course:Programming 2B
Project
POE PART 2: Functional Implementation and Unit Testing
🧭 Table of Contents
Executive Summary and Functional Implementation

1.1. Project Overview and Functional Design

1.2. Database Structure and Data Persistence

1.3. GUI/UI Layout Implementation and Functionality

Data Modeling
2.1. UML Class Diagram and Key Relationships

Unit Testing and Quality Assurance

3.1. ClaimService Tests (8 Total)

3.2. ClaimsController Tests (9 Total)

3.3. Assumptions, Constraints, and Testing Scope

Project Management (Historical)

4.1. Historical Project Plan

Submission Details & Links

References

1. Executive Summary and Functional Implementation

The Contract Monthly Claim System (CMCS) has been developed as a fully functional web application using the ASP.NET Core MVC framework. The continued use of the Model-View-Controller (MVC) architectural pattern ensures a robust separation of concerns, which enhances the system's maintainability and scalability.

The core objective of CMCS is to streamline the monthly claim submission and approval process for contract lecturers. Implementation in Part 2 focused on integrating core business logic, data persistence, and Role-Based Authorization (RBA) necessary for operational security.

1.1. Project Overview and Functional Design

Strategy
Detail
Technology
ASP.NET Core MVC (C#)
Data Persistence

Entity Framework Core (EF Core) as the ORM to manage CRUD operations.
Role-Based Security
ASP.NET Identity enforces strict RBA for Lecturer, Programme Coordinator, and Academic Manager.
Business Logic
Server-side logic in the ClaimsController calculates the total claim amount (HoursWorked * HourlyRate) and manages the status update lifecycle.

1.2. Database Structure and Data Persistence
The structure, defined in Part 1, was instantiated using EF Core migrations.

Claim Entity: Central to the system, storing metadata, claim period, and the critical Status property (an enumerated type) that controls visibility and progression.

Document Entity: Maintains a one-to-many relationship with Claim. Tracks OriginalFileName and the secure SupportingDocumentPath, linking evidence to the correct claim record.

All data manipulation is handled exclusively through asynchronous methods provided by EF Core.

1.3. GUI/UI Layout Implementation and Functionality

The user interface, constructed with Bootstrap, provides specific and functional workflows for each user role.

Claim Submission and File Upload (Lecturer)

Form Security: Configured with enctype="multipart/form-data" for document uploads.
Secure File Handling: Server-side code processes the IFormFile stream, enforcing restrictions on file types (PDF/DOCX) and size (5MB limit), saving the file to a secure local path.

Initial Status: Claims are persisted with the initial status of PendingReview.

Review and Approval Workflows (Coordinator and Manager)
Dynamic Dashboards: Dashboards display only the claims relevant to the current user's approval step.

Secure Actions: Actions use dedicated HTML <form> elements with method="post", secured by Anti-Forgery Tokens.
Coordinator Actions: Enabled only for claims with Status == ClaimStatus.PendingReview.
Manager Actions: Enabled only for claims with Status == ClaimStatus.VerifiedByCoordinator.
Claim Details and Status Tracking

The ViewDetails.cshtml page uses a Razor C# @switch statement to dynamically inspect the Model.Status enum and apply corresponding Bootstrap classes and icons (e.g., bg-warning, bg-success) for instant visual communication of the claim's status.

2. Data Modeling

2.1. UML Class Diagram and Key Relationships

The UML Class Diagram serves as the structural blueprint for the application's persistent data layer.

Key Relationships:

User to Claim (1:M): A single Lecturer (User) can submit multiple claims. This is enforced by the LecturerId foreign key within the Claim entity.

Claim to Document (1:M): Each claim can be supported by multiple documents. The ClaimId foreign key in the Document entity ensures all supporting evidence is directly linked.

3. Unit Testing and Quality Assurance

The core functionality is validated through 17 unit tests across two files: ClaimServiceTests.cs (business logic) and ClaimsControllerTests.cs (user interaction/routing).

3.1. ClaimService Tests (8 Total)

These tests validate the business logic and data manipulation within the ClaimService.

Test Method Name

Purpose

Key Assertions

CreateClaimAsync_WithValidData_ReturnsClaimId

Ensures successful claim creation.

Returns Claim ID $> 0$; initial status set to PendingReview.

CreateClaimAsync_WithNullDocument_ThrowsArgumentException

Validates rejection if a document is missing.

Throws ArgumentException (document is required).

CreateClaimAsync_WithOversizedFile_ThrowsArgumentException

Checks the 5MB file size restriction.

Throws ArgumentException (size limit exceeded).

CreateClaimAsync_WithInvalidFileType_ThrowsArgumentException

Verifies rejection of non-permitted file types.

Throws ArgumentException (invalid file type).

GetClaimByIdAsync_WithExistingId_ReturnsClaim

Confirms a created claim can be retrieved correctly.

Returns a non-null Claim object matching the requested ID.

UpdateClaimStatusAsync_WithValidId_UpdatesStatus

Validates the core status update mechanism.

Asserts the status is the newly set value (VerifiedByCoordinator).

GetPendingClaimsAsync_ForCoordinator_ReturnsOnlyPendingClaims

Ensures the Coordinator dashboard retrieves only PendingReview claims.

Returns a list where all claims have the status PendingReview.

GetManagerDashboardDataAsync_ReturnsCorrectCounts

Verifies correct aggregation of total claim counts.

Returns a dashboard object where TotalClaimsCount $> 0$.

3.2. ClaimsController Tests (9 Total)

These tests validate the controller's actions, focusing on correct routing, view rendering, and form submission handling.

Test Method Name
Purpose
Key Assertions
Index_ReturnsViewWithClaims

Checks the Lecturer's index page loads and passes claims to the view.

Returns a ViewResult with the user's claims list as the model.

Create_Get_ReturnsView

Ensures the HTTP GET for claim creation returns the expected view.

Returns a ViewResult with view name ~/Views/Claim/Create.cshtml.

Create_Post_WithValidModel_RedirectsToIndex

Verifies successful submission.

Returns a RedirectToActionResult to the Index action.

ViewDetails_WithValidId_ReturnsView

Ensures successful viewing of claim details by ID.

Returns a ViewResult with view name ~/Views/Claim/ViewDetails.cshtml and a non-null Claim model.

ViewDetails_WithInvalidId_RedirectsToHome

Tests the failure case for viewing details.

Returns a RedirectToActionResult to the Home/Index page.

Verify_WithValidId_UpdatesStatusAndRedirects

Checks the Coordinator's verification action.

Calls UpdateClaimStatusAsync (VerifiedByCoordinator), redirects to Home/CoordinatorDashboard.

Approve_WithValidId_UpdatesStatusAndRedirects
Checks the Manager's approval action.
Calls UpdateClaimStatusAsync (ApprovedByManager), redirects to Home/ManagerDashboard.
Reject_AsCoordinator_UpdatesStatusAndRedirects

Tests the Coordinator's rejection action.

Calls UpdateClaimStatusAsync (Rejected), redirects to Home/CoordinatorDashboard.

Reject_AsManager_UpdatesStatusAndRedirects

Tests the Manager's rejection action.

Calls UpdateClaimStatusAsync (Rejected), redirects to Home/ManagerDashboard.

3.3. Assumptions, Constraints, and Testing Scope

Category

Detail

Assumptions

ASP.NET Identity is properly configured, and all users have been correctly assigned one of the three required roles.

Constraints

File handling is restricted to local server path storage (no cloud integration). Audit logging is limited to high-level claim status changes.

Testing Focus

Strictly constrained to the core business logic (calculation) and the state transition logic within the ClaimsController.

4. Project Management (Historical)

4.1. Historical Project Plan

The table below outlines the original project plan used to guide development through both Part 1 and Part 2.
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

1.1
Week 1



1.3.
Design the Database Schema (UML Class Diagram)

1.2
Week 2

1.4.

Implement ASP.NET Identity and Role-Based Setup

1.2

Week 2

1.5.

Develop the Non-Functional Prototyping Views (GUI/UI)

1.4

Week 3

Phase 2: Functional Implementation

2.1.

Implement Entity Framework Core and Database Migrations

1.3

Week 4



2.2.

Implement the Claim Submission and File Upload Functionality (Lecturer)

2.1

Week 4



2.3.

Implement Coordinator Verification and Rejection Actions

2.1, 2.2

Week 5



2.4.

Implement Manager Approval and Rejection Actions

2.3

Week 6



2.5.

Implement Unit Tests for Business Logic (Calculation & Status)

2.2, 2.3

Week 6

Phase 3: Finalization & Submission

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

5. Submission Details & Links

The source code and documentation for this project are hosted on a publicly accessible GitHub repository, and the system demonstration is provided via an unlisted YouTube video.

Resource Link

GitHub Repository

https://github.com/ThembaTshudufhadzo/PROG6212POEPART2-CMCSWebApp.git
YouTube Video Demo
https://youtu.be/GKdE6K-KUks

6. References Type

Reference

Book
Freeman, A. and Sandell, J. (2018). Pro ASP.NET Core MVC 2. 7th ed. Berkeley, CA: Apress.
Website
GitHub. (2025). Git and GitHub Documentation. Available at: https://docs.github.com/en/ (Accessed: 16 September 2025).

Website
Mermaid. (2025). Mermaid Documentation. Available at: https://mermaid.js.org/documentation/ (Accessed: 16 September 2025).
Website
Microsoft. (2025a). Introduction to ASP.NET Core MVC. Available at: https://docs.microsoft.com/en-us/aspnet/core/mvc/overview (Accessed: 15 October 2025).
Website
Microsoft. (2025b). Entity Framework Core Documentation. Available at: https://docs.microsoft.com/en-us/ef/core/ (Accessed: 15 October 2025).
