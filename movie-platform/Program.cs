using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.EntityFrameworkCore;
using movie_platform.Models;
using movie_platform.Services;
using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MovieplatformdbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection2RDS")));

// Configure DynamoDB
builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    var config = builder.Configuration;
    var awsCredentials = new BasicAWSCredentials(
        config["AWS:AccessKey"],
        config["AWS:SecretKey"]
    );
    var dynamoConfig = new AmazonDynamoDBConfig
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(config["AWS:Region"])
    };

    return new AmazonDynamoDBClient(awsCredentials, dynamoConfig);
});

// Add DynamoDB context as a singleton service
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>(sp =>
{
    var dynamoDbClient = sp.GetRequiredService<IAmazonDynamoDB>();
    return new DynamoDBContext(dynamoDbClient);
});

// Add DynamoDB context and DynamoDBMovieOperation as singleton services
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>(sp =>
{
    var dynamoDbClient = sp.GetRequiredService<IAmazonDynamoDB>();
    return new DynamoDBContext(dynamoDbClient);
});
builder.Services.AddSingleton<DynamoDBMovieOperation>();

// Configure S3
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = builder.Configuration;
    var awsCredentials = new BasicAWSCredentials(
        config["AWS:AccessKey"], // if doesnt work, add new s3 access like AWSS3:AccessKey  
        config["AWS:SecretKey"]
    );
    var s3Config = new AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(config["AWS:Region"])
    };

    return new AmazonS3Client(awsCredentials, s3Config);
});

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make cookie accessible only through HTTP requests
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

var app = builder.Build();

// Run CreateMovieTableAsync during application startup
using (var scope = app.Services.CreateScope())
{
    var dbOperation = scope.ServiceProvider.GetRequiredService<DynamoDBMovieOperation>();
    await dbOperation.CreateMovieTableWithIndexesAsync();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
