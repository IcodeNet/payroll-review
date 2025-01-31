[⬅️ Back to Documentation](../../../README.md)

# Unit Testing Issues - Fixed Implementation

## 1. Business Logic Tests - Fixed

```csharp
[TestFixture]
public class SalaryCalculatorTests
{
    private SalaryCalculator _calculator;
    private TestEmployeeBuilder _employeeBuilder;

    [SetUp]
    public void Setup()
    {
        _calculator = new SalaryCalculator();
        _employeeBuilder = new TestEmployeeBuilder();
    }

    [Test]
    [TestCase(100000, 0.2, true, 89000, Description = "High salary with bonus")]
    [TestCase(100000, 0.2, false, 80000, Description = "High salary without bonus")]
    [TestCase(40000, 0.2, true, 36500, Description = "Low salary with bonus")]
    [TestCase(40000, 0.2, false, 32000, Description = "Low salary without bonus")]
    public void CalculateNetSalary_WithValidInputs_ReturnsExpectedAmount(
        decimal gross,
        decimal taxRate,
        bool includeBonus,
        decimal expected)
    {
        // Act
        var result = _calculator.CalculateNetSalary(gross, taxRate, includeBonus);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void CalculateNetSalary_WithNegativeGross_ThrowsArgumentException()
    {
        // Arrange
        const decimal negativeGross = -1000m;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            _calculator.CalculateNetSalary(negativeGross, 0.2m, false));

        Assert.That(ex.Message, Does.Contain("must be positive"));
    }

    [Test]
    public void CalculateNetSalary_WithInvalidTaxRate_ThrowsArgumentException()
    {
        // Arrange
        const decimal invalidTaxRate = 1.5m;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            _calculator.CalculateNetSalary(1000m, invalidTaxRate, false));

        Assert.That(ex.Message, Does.Contain("between 0 and 1"));
    }
}
```

## 2. Repository Tests - Fixed

```csharp
[TestFixture]
public class EmployeeRepositoryTests
{
    private SqliteConnection _connection;
    private DbContextOptions<PayrollContext> _options;
    private PayrollContext _context;
    private EmployeeRepository _repository;
    private TestEmployeeBuilder _employeeBuilder;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<PayrollContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new PayrollContext(_options);
        context.Database.EnsureCreated();
    }

    [SetUp]
    public void Setup()
    {
        _context = new PayrollContext(_options);
        _repository = new EmployeeRepository(_context);
        _employeeBuilder = new TestEmployeeBuilder();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _connection.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_WithExistingEmployee_ReturnsEmployee()
    {
        // Arrange
        var employee = _employeeBuilder
            .WithDefaultValues()
            .Build();

        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(employee.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(employee.Id));
        Assert.That(result.Name, Is.EqualTo(employee.Name));
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistentEmployee_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.That(result, Is.Null);
    }
}
```

## 3. Service Layer Tests - Fixed

```csharp
[TestFixture]
public class EmployeeServiceTests
{
    private Mock<IEmployeeRepository> _mockRepo;
    private Mock<IValidator<CreateEmployeeCommand>> _mockValidator;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<EmployeeService>> _mockLogger;
    private EmployeeService _service;
    private TestEmployeeBuilder _employeeBuilder;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IEmployeeRepository>();
        _mockValidator = new Mock<IValidator<CreateEmployeeCommand>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<EmployeeService>>();
        _service = new EmployeeService(
            _mockRepo.Object,
            _mockValidator.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
        _employeeBuilder = new TestEmployeeBuilder();
    }

    [Test]
    public async Task CreateEmployeeAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateEmployeeCommand("John Doe", "IT", 50000);
        var employee = _employeeBuilder
            .WithName("John Doe")
            .WithDepartment("IT")
            .WithSalary(50000)
            .Build();

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateEmployeeAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateEmployeeAsync_WithInvalidCommand_ReturnsFailure()
    {
        // Arrange
        var command = new CreateEmployeeCommand("", "", -1);
        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Salary", "Salary must be positive")
        };

        _mockValidator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _service.CreateEmployeeAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Does.Contain("Name is required"));
        Assert.That(result.Error, Does.Contain("Salary must be positive"));
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

## 4. Integration Tests - Fixed

```csharp
[TestFixture]
public class EmployeeIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public EmployeeIntegrationTests(
        WebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<PayrollContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<PayrollContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
        _output = output;
    }

    [Test]
    public async Task CreateEmployee_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new CreateEmployeeCommand("John Doe", "IT", 50000);

        // Act
        var response = await _client.PostAsJsonAsync("/api/employees", command);
        var content = await response.Content.ReadFromJsonAsync<EmployeeDto>();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Name, Is.EqualTo("John Doe"));
    }

    [Test]
    public async Task GetEmployee_WithExistingId_ReturnsEmployee()
    {
        // Arrange
        var command = new CreateEmployeeCommand("Jane Doe", "HR", 60000);
        var createResponse = await _client.PostAsJsonAsync("/api/employees", command);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();

        // Act
        var response = await _client.GetAsync($"/api/employees/{created.Id}");
        var content = await response.Content.ReadFromJsonAsync<EmployeeDto>();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Id, Is.EqualTo(created.Id));
    }
}
```

## Key Improvements

1. **Test Organization**
   - Clear test structure
   - Proper setup/teardown
   - Descriptive names
   - Consistent patterns

2. **Test Coverage**
   - Business logic tests
   - Repository tests
   - Service layer tests
   - Integration tests

3. **Test Data**
   - Test data builders
   - Meaningful test cases
   - Edge case coverage
   - Clear arrangements

4. **Assertions**
   - Clear assertions
   - Multiple validations
   - Proper error checks
   - Behavior verification

5. **Testing Best Practices**
   - Dependency isolation
   - Database testing
   - Mocking
   - Integration testing