📄 Smart Document OCR Organizer

📌 Project Overview

Smart Document OCR Organizer is a secure web-based application that allows users to upload scanned documents or PDFs, extract text using OCR (Optical Character Recognition), automatically categorize documents, and manage them efficiently through authentication.

This project was built as an academic full-stack web application using ASP.NET Core MVC, Entity Framework Core (Code First), and SQL Server.

🎯 Features

🔐 User Authentication

Register, Login, Logout

Password hashing with ASP.NET Identity

📤 Document Upload

Upload scanned images and PDF files

🔎 OCR Text Extraction

Text extraction using Tesseract OCR Engine

🗂 Automatic Categorization

Keyword-based classification (Invoices, Receipts, IDs, Letters)

🔍 Searchable Documents

Search and filter extracted documents

🔄 Password Reset (Local)

Simple reset flow without external email dependency (school-friendly)

🧰 Tech Stack Layer Technology Frontend ASP.NET Core MVC, Razor Pages, Bootstrap Backend C# (.NET) OCR Tesseract OCR Database SQL Server LocalDB ORM Entity Framework Core (Code First) Auth ASP.NET Core Identity 🗄 Database (Code First)

Database schema is generated from C# models

Entity Framework Core migrations manage schema changes

No manual SQL scripting required

Core Tables

AspNetUsers – User authentication data

Documents – Uploaded files and OCR text

Categories – Document categories

DocumentCategories – Mapping relationship

⚙️ How to Run the Project ✅ Requirements

Visual Studio 2022+

.NET SDK (net9.0)

SQL Server LocalDB

Windows OS (recommended for OCR)

▶️ Setup Steps

Clone or download the repository

git clone https://github.com/AOtu12/SmartDocOCR_Proj01.git

Open the solution in Visual Studio

Verify appsettings.json:

"DefaultConnection": "Server=(localdb)\MSSQLLocalDB;Database=SmartDocOcrDb;Trusted_Connection=True;"

Open Package Manager Console

Run:

Update-Database

Run the project

🔐 Authentication Flow

User registers an account

User logs in securely

Authentication handled with cookies via ASP.NET Identity

Password reset handled internally (no SMTP dependency)

🧠 OCR Workflow

User uploads a document

File is stored on the server

Tesseract OCR extracts text

Extracted text is saved to database

Keywords determine category

Document becomes searchable

🛡 Security Highlights

Password hashing & salting

User-specific document access

Server-side validation

Prepared for future MFA or JWT integration

👥 Contributors

Akosua Otu

Backend development

OCR integration

Database design

Categorization logic

Israel Odubona

Frontend UI

Search & filtering

Analytics

Testing & documentation

🚀 Future Enhancements

Multi-Factor Authentication (MFA)

API access with JWT

Cloud storage

Improved OCR accuracy

Advanced analytics dashboard

🎓 Academic Context

Developed for CPRO 2211 – Web Application using C#.NET Demonstrates:

Code-First database design

Secure authentication

OCR integration

Real-world web application architecture
