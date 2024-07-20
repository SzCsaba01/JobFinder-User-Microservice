# Job Recommendation System

The Job Recommendation System is designed to revolutionize the job search process by leveraging advanced data extraction and machine learning techniques. This system aims to provide users with highly relevant job recommendations based on their CVs and interactions with job postings.

## Objectives

- **Enhance Job Search Efficiency:** Reduce the time and effort required for users to find suitable job opportunities.
- **Improve Job Matching Accuracy:** Utilize sophisticated algorithms to match users with jobs that closely align with their skills, experience, and preferences.
- **Provide a Seamless User Experience:** Create an intuitive and user-friendly interface for job seekers.

## Architecture

The system is built using a microservices architecture, ensuring scalability and maintainability. Key components include:

- **Job Aggregator Service:** Collects job postings from multiple sources such as Jobicy, Remotive, and Adzuna.
- **CV Parsing Service:** Uses the OpenAI API to extract relevant information from user-uploaded CVs.
- **Recommendation Service:** Analyzes both static CV data and dynamic user interactions to generate personalized job recommendations using the OpenAI API.
- **User Interface:** A web-based application that allows users to upload CVs, browse job listings, save jobs, and receive recommendations.

## Data Flow

1. **CV Upload:** Users upload their CVs in PDF or DOCX format.
2. **Data Extraction:** The CV Parsing Service extracts key information such as skills, experience, and education.
3. **Job Aggregation:** The Job Aggregator Service collects job postings from various sources.
4. **Recommendation Service:** Combines CV data and user interactions to generate personalized job recommendations using the OpenAI API.
5. **User Interaction:** Users can save job listings, view external sources, and provide feedback.

## Technologies Used

- **Backend:** .NET 6
- **Frontend:** Angular
- **Database:** MS SQL
- **APIs:** OpenAI API for CV parsing and job recommendations, various job aggregator APIs

## Repositories


- **Job Microservice:** https://github.com/SzCsaba01/JobFinder-Job-Microservice
- **User Microservice:** https://github.com/SzCsaba01/JobFinder-User-Microservice
- **Location Microservice:** https://github.com/SzCsaba01/JobFinder-Location-Microservice
- **FrontEnd:** https://github.com/SzCsaba01/JobFinder-FrontEnd
