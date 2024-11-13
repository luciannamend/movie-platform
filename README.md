This is a web application designed to be a movie platform. Developed using ASP.NET and deployed on AWS Elastic Beanstalk, it allows registered users to download, manage, and rate movies. Users can also view and interact with comments on each movie, rate movies, and search for content based on genre or ratings.

**Features**

  1.User Authentication
   - User Registration: Users can create an account, with their information securely stored in AWS RDS using Microsoft SQL Server.
   - User Login: Registered users can log in to access platform features.
  2.Movie Management
   - Add Movie: Users can add movies into the platform.
   - Delete Movie: Users can delete movies they have added.
   - Modify Movie Metadata: Users can update information such as title, genre, director, and release year.
   - Download Movie: Users can download any movie stored in S3.
   - List Movies by Rating: View movies with a rating above a certain threshold (e.g., movies with a rating > 9).
   - List Movies by Genre: Filter movies by genre to easily find content of interest (e.g., Science Fiction).
  3.Comments and Ratings
   - Add Comments: Users can add comments to a movie.
   - Rate Movies: Users can rate movies, contributing to an overall average rating for each movie.
   - View Comments and Ratings: Users can view all comments and the average rating for each movie.  
  4.Search and Filter
   - Search by Rating: Use a secondary index in DynamoDB to list movies with an overall rating above a specific value.
   - Search by Genre: Use a secondary index in DynamoDB to filter movies by genre, making it easy to browse by movie type.

**Technologies Used**
 - ASP.NET Core: For building the web application.
 - AWS Elastic Beanstalk: Deployment and scaling of the web application.
 - AWS RDS (Microsoft SQL Server): For storing user registration and login information.
 - AWS S3: To store and serve movie files.
 - AWS DynamoDB: For storing movie metadata, comments, and ratings.
   - Primary Key: Movie ID.
   - Secondary Indexes:
     - Rating index for quick filtering of movies with a high rating.
     - Genre index for filtering movies by genre.
    
**How to use**

 1.Register and Log In: Users can register and log in to access the movie platform.
 
 2.Manage Movies:
   - Add a movie, including metadata like title, genre, and release year.
   - Delete or modify movies (only available to the user who uploaded the movie).
   - Download any movie stored in the platform.

 3.Comments and Ratings:
   - Add comments and rate movies.
   - View a list of comments and the average rating on each movie’s page.

 4.Search and Filter:
   - Filter movies based on genre or ratings using the search functionality.


**Disclaimer**

Please note that this project is intended for demonstration purposes only. I do not maintain the movie video files on AWS S3 buckets, nor do I store user and movie data long-term, as this would incur ongoing storage and maintenance costs. Users interested in deploying this project will need to set up and fund their own AWS services, including S3 for video storage, RDS for user data, and DynamoDB for movie metadata.
