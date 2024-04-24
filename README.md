# Technical test solution

Hello, I am Sebastian Gomez Quintero and below you will see the solution to the technical test


# Answer question 1
The answer to this point is below in sql. The EF part can be viewed during execution in the project by starting the API or running the unit test.

## Proposed model for the data

    CREATE TABLE Car (
        CarId INT PRIMARY KEY IDENTITY(1,1),
        Year INT,
        Make NVARCHAR(100),
        Model NVARCHAR(100),
        Submodel NVARCHAR(100),
        ZipCode NVARCHAR(10)
    );
    CREATE TABLE Buyer (
        BuyerId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100)
    );
    CREATE TABLE Quote (
        QuoteId INT PRIMARY KEY IDENTITY(1,1),
        CarId INT,
        BuyerId INT,
        Amount DECIMAL(10, 2),
        IsCurrent BIT,
        FOREIGN KEY (CarId) REFERENCES Car(CarId),
        FOREIGN KEY (BuyerId) REFERENCES Buyer(BuyerId)
    );   
    CREATE TABLE Status (
        StatusId INT PRIMARY KEY IDENTITY(1,1),
        CarId INT,
        StatusName NVARCHAR(100),
        StatusDate DATETIME,
        ChangedBy NVARCHAR(100),
        FOREIGN KEY (CarId) REFERENCES Car(CarId)
    );


## Query to display the information

    WITH LatestStatus AS (SELECT CarId, MAX(StatusId) AS MaxStatusId  
                      FROM Status  
	 GROUP BY CarId)  
	SELECT c.Year,  
	  c.Make,  
	  c.Model,  
	  c.Submodel,  
	  b.Name AS BuyerName,  
	 q.Amount AS QuoteAmount,  
	 s.StatusName,  
	 s.StatusDate  
	FROM Car c  
	         INNER JOIN Quote q ON c.CarId = q.CarId AND q.IsCurrent = 1  
	  INNER JOIN Buyer b ON q.BuyerId = b.BuyerId  
	  INNER JOIN Status s ON c.CarId = s.CarId  
	  INNER JOIN LatestStatus ls ON s.CarId = ls.CarId AND s.StatusId = ls.MaxStatusId;
## Entity Framework query

    public async Task<IEnumerable<CarInfoDto>> GetCarInformationAsync()
	{
      var cars = await _context.Cars.ToListAsync();

      var currentQuotes = await _context.Quotes
          .Where(q => q.IsCurrent)
          .Select(q => new { q.CarId, q.Buyer.Name, q.Amount })
          .ToListAsync();

      var latestStatuses = await _context.Statuses
         .GroupBy(s => s.CarId)
         .Select(g => new { g.Key, Status = g.OrderByDescending(s => s.StatusId).FirstOrDefault() })
         .ToListAsync();

      var result = cars
            .Select(c => new CarInfoDto
            {
                Year = c.Year,
                Make = c.Make,
                Model = c.Model,
                Submodel = c.Submodel,
                BuyerName = currentQuotes.FirstOrDefault(q => q.CarId == c.CarId).Name,
                QuoteAmount = currentQuotes.FirstOrDefault(q => q.CarId == c.CarId).Amount,
                StatusName = latestStatuses.FirstOrDefault(ls => ls.Key == c.CarId).Status.StatusName,
                StatusDate = latestStatuses.FirstOrDefault(ls => ls.Key == c.CarId).Status.StatusDate
            });

      return result;
     }

# Answer question 2

What would you do if you had data that doesn't change often but it's used pretty much all the time?

For data that doesn't change often but is frequently accessed, implementing a caching strategy is often the best approach. Caching involves storing data in a fast-access data store after the first retrieval so that future requests for the data can be served quickly, without the need for repeated database queries.

There are several caching strategies to consider:

1. **In-Memory Caching**: Storing data directly in the application's memory can significantly speed up access. Technologies like Redis, Memcached, or even in-process caching with a tool like MemoryCache in .NET can be used.
2. **Distributed Caching**: For applications that scale horizontally across multiple servers, a distributed cache ensures that cached data is available to all instances of the application. This approach is beneficial for cloud-native applications that require high availability and scalability.
3. **Persistent Caching**: Sometimes data might need to be cached for longer periods and should persist beyond application restarts. In such cases, using a persistent store like a file-based cache or a database can be suitable.
4. **Cache Invalidation**: It’s crucial to have a strategy for invalidating the cache when the data finally does change. This can be done through expiration policies, version tags, or event-driven cache refreshes.
5. **CDN Caching**: For web applications, static data such as images, CSS, and JavaScript files can be cached in a Content Delivery Network (CDN) to improve load times for users across different geographical locations.

Caching can reduce the load on the database, decrease latency, and improve the overall responsiveness of an application.

# Answer question 3

## Refactoring of the UpdateCustomersBalanceByInvoices method

### Original 

	public void UpdateCustomersBalanceByInvoices(List<Invoice> invoices)
	{
	    foreach (var invoice in invoices)
	    {
	        var customer = dbContext.Customers.SingleOrDefault(c => c.Id == invoice.CustomerId);
	        customer.Balance -= invoice.Amount;
	        dbContext.SaveChanges();
	    }
	}

This method has several issues that could be optimized:

1.  **Performance**: The method is calling `dbContext.SaveChanges()` inside a loop, which results in a separate database transaction for each invoice. This is highly inefficient.
    
2.  **Error Handling**: There's no error handling to ensure that the `customer` is found before trying to update the balance.
    
3.  **Multiple Database Calls**: The method is retrieving the customer from the database one by one in a loop, which can lead to performance issues due to the number of database calls.
    
4.  **No Concurrency Handling**: If the application is used in a multi-threaded environment, the lack of concurrency handling could lead to inconsistent data.


	
	    public void UpdateCustomersBalanceByInvoices(List<Invoice> invoices)
	    {
	    var customerIds = invoices.Select(i => i.CustomerId).Distinct().ToList();
	    var customers = dbContext.Customers
	                             .Where(c => customerIds.Contains(c.Id))
	                             .ToList();
	    
	    foreach (var invoice in invoices)
	    {
	        var customer = customers.SingleOrDefault(c => c.Id == invoice.CustomerId);
	        if (customer != null)
	        {
	            customer.Balance -= invoice.Amount;
	        }
	        else
	        {
	            // Handle the case where the customer is not found
	            // This could be logging the error or throwing an exception
	        }
	    }

	    dbContext.SaveChanges();
        }

In the improved method:

-   The database context `SaveChanges` is called once after all updates, which reduces the number of transactions and improves performance.
-   Customers are retrieved in a single query, which reduces database round trips.
-   The customer existence check is added to avoid null reference exceptions.
-   It prepares the method for a potential batch update optimization and better error handling.

# Answer question 4

## Implementation of the GetOrders method

	public async Task<List<OrderDTO>> GetOrders(DateTime? dateFrom, DateTime? dateTo, List<int>? customerIds, List<int>? statusIds, bool? isActive)
	{
    // Start with a queryable that includes any necessary joins
    var query = _context.Orders.AsQueryable();

    // Apply filtering for dateFrom if provided
    if (dateFrom.HasValue)
    {
        query = query.Where(order => order.Date >= dateFrom.Value);
    }

    // Apply filtering for dateTo if provided
    if (dateTo.HasValue)
    {
        query = query.Where(order => order.Date <= dateTo.Value);
    }

    // Apply customer ID filtering if any customer IDs are provided
    if (customerIds != null && customerIds.Count > 0)
    {
        query = query.Where(order => customerIds.Contains(order.CustomerId));
    }

    // Apply status ID filtering if any status IDs are provided
    if (statusIds != null && statusIds.Count > 0)
    {
        query = query.Where(order => statusIds.Contains(order.StatusId));
    }

    // Apply active status filtering if an isActive filter is provided
    if (isActive.HasValue)
    {
        query = query.Where(order => order.IsActive == isActive.Value);
    }

    // Project the results to the DTO
    var orders = await query.Select(order => new OrderDTO
    {
        OrderId = order.Id,
        CustomerId = order.CustomerId,
        StatusId = order.StatusId,
        IsActive = order.IsActive,
        // Additional properties may be added here
    }).ToListAsync();

    return orders;
}


# Answer question 5

Bill, from the QA Department, assigned you a high priority task indicating there’s a bug when someone changes the status from “Accepted” to “Picked Up”. Define how you would proceed, step by step, until you create the Pull Request

### Step-by-Step Approach to Resolve the Bug

1.  **Reproduce the Issue**:
    
	  -   Begin by trying to reproduce the bug in a controlled development environment. Use the exact steps provided by the QA team or the steps that lead to the error as reported.
	  -   Verify that the error occurs consistently with the described steps and under similar conditions.
2.  **Identify the Bug**:
    
    -   Examine the code responsible for changing the status from "Accepted" to "Picked Up". This likely involves event handlers or services that manage state transitions.
    -   Check for any conditions, validations, or errors in the logic that could lead to the reported issue.
3.  **Debugging**:
    
    -   Use debugging tools to step through the code and monitor the state of the application as it transitions from "Accepted" to "Picked Up".
    -   Pay special attention to any modifications that are made to the database or application state during this transition.
4.  **Write a Test**:
    
    -   Write a unit test that fails due to the bug. This not only confirms the presence of the bug but also ensures that once fixed, the bug does not reoccur.
    -   The test should mimic the state change and check for the expected outcome.
5.  **Implement the Fix**:
    
    -   Once the source of the bug is identified, modify the code to rectify the issue. This might involve correcting the logic for the state transition, handling exceptions more appropriately, or adjusting related business logic.
    -   Ensure that the changes do not break any existing functionality by running existing tests.
6.  **Run Tests**:
    
    -   Run the newly created test along with other relevant tests to ensure that the fix works as expected and does not introduce new issues.
    -   Make sure all tests pass before proceeding to the next steps.
7.  **Code Review**:
    
    -   Create a pull request (PR) for the changes. Provide a clear description of the changes and the reason for them in the PR.
    -   Request a code review from your peers. Ensure that at least one other developer reviews the changes to ensure quality and maintainability.
8.  **Deploy the Changes**:
    
    -   Once approved, merge the PR into the main development branch.
    -   Follow your team’s standard procedures for deploying changes to the production environment, which might involve further stages of testing.
9.  **Verify the Fix in Production**:
    
    -   Once deployed, verify that the fix resolves the issue in the production environment without causing unexpected side effects.
10.  **Document the Resolution**:    

     - Update any documentation that reflects the changes made. This might include technical documentation, user manuals, or release notes.
	  -   Documenting helps future developers understand the changes and reasons behind them, which is beneficial for long-term maintenance.


**Conclusion**

Thank you for the opportunity to complete this technical assessment. I enjoyed the challenges presented and found them to be a valuable exercise in applying my skills and knowledge. I believe the solutions I provided align with the requirements specified and demonstrate my ability to effectively solve problems using my technical expertise.

Should there be any part of my submission that requires clarification, or if further information is needed, please do not hesitate to contact me. I look forward to the possibility of discussing my application in further detail.

Best regards,

Sebastian Gomez Quintero.