var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Используем CORS
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// 2.2 Изначальный список продуктов
var products = new List<Product>
{
    new Product(1, "IPhone"),
    new Product(2, "MacBook"),
    new Product(3, "Headphones")
};

// 1. Конечная точка для получения баланса
app.MapGet("/api/balance", () =>
{
    var balance = 1500.75m;
    return Results.Ok(balance);
});

// 3.1 Получать весь список продуктов
app.MapGet("/api/products", () =>
{
    return Results.Ok(products);
});

// 3.2 Получать продукт по Id
app.MapGet("/api/products/{id}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product == null)
    {
        return Results.NotFound($"Продукт с ID {id} не найден");
    }
    return Results.Ok(product);
});

// 3.3 Добавлять новые продукты
app.MapPost("/api/products", (Product product) =>
{
    // Проверка входных данных
    if (product == null)
    {
        return Results.BadRequest("Данные продукта не могут быть пустыми");
    }

    if (string.IsNullOrWhiteSpace(product.Name))
    {
        return Results.BadRequest("Название продукта не может быть пустым");
    }

    // Проверка на существующий ID
    if (products.Any(p => p.Id == product.Id))
    {
        return Results.BadRequest($"Продукт с ID {product.Id} уже существует");
    }

    products.Add(product);
    return Results.Created($"/api/products/{product.Id}", product);
});

// 3.4 Изменять название по Id
app.MapPut("/api/products/{id}", (int id, ProductUpdateRequest request) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product == null)
    {
        return Results.NotFound($"Продукт с ID {id} не найден");
    }

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Название продукта не может быть пустым");
    }

    product.Name = request.Name;
    return Results.Ok(product);
});

// 3.5 Удалять по Id
app.MapDelete("/api/products/{id}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product == null)
    {
        return Results.NotFound($"Продукт с ID {id} не найден");
    }

    products.Remove(product);
    return Results.Ok($"Продукт с ID {id} удален");
});

app.Run();

// 2.1 Модель Product
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Product() { }

    public Product(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

// Модель для обновления продукта
public class ProductUpdateRequest
{
    public string Name { get; set; } = string.Empty;
}