using System.Globalization;
using Scriban;
using Scriban.Runtime;
using ScribanSample;

var template = Template.Parse("Hello {{ name }}!");
var result = await template.RenderAsync(new { Name = "World" });

Console.WriteLine(result);
Console.WriteLine();

var people = new List<Person>
{
    new("Marco", "Minerva", "Taggia"),
    new("Donald", "Duck", "Paperopoli"),
    new("Scrooge", "McDuck", "Paperopoli"),
};

template = Template.Parse("""
    <ul id="people">
        {{- for person in Model.People }}
        <li>
            {{ person.FirstName }} {{ person.LastName }} from {{ person.City }}
        </li>
        {{- end }}
    </ul>
    """);

var context = CreateContext(new PeopleModel { People = people });
result = await template.RenderAsync(context);

Console.WriteLine(result);
Console.WriteLine();

var products = new List<Product>()
{
    new("Apple", 1.99m, "A really delicious apple", DateTime.Now.AddDays(-1)),
    new("Banana", 0.99m, "A yellow banana", DateTime.Now.AddDays(-5)),
    new("Orange", 1.49m, "A juicy and big orange", DateTime.Now.AddDays(42)),
};

template = Template.Parse("""
    <ul id="products">
        {{- for product in Model.Products }}
        <li>
            <h2>{{ product.Name }}</h2>
            Price: {{ product.Price | object.format "C" }}
            {{ product.Description | string.truncate 15 }}
            {{ product.Date | date.to_string '%d/%m/%Y %R' }}
        </li>
        {{- end }}
    </ul>
    """);

context = CreateContext(new ProductsModel { Products = products });
result = await template.RenderAsync(context);

Console.WriteLine(result);
Console.WriteLine();

template = Template.Parse("""
    <html>
    <body>
    <p>Hello {{ Model.Name }},</p>
    <p>
    Thank yout for registering. Please <a href="{{ Model.Url }}">click here</a> to activate your account
    </p>
    </body>
    </html>
    """);

context = CreateContext(new ConfirmationEmailModel { Name = "Marco", Url = "https://www.example.com" });
result = await template.RenderAsync(context);

Console.WriteLine(result);
Console.WriteLine();

result = await OrderConfirmation.CreateOrderAsync();

Console.WriteLine(result);

static TemplateContext CreateContext(object model)
{
    var context = new TemplateContext { MemberRenamer = member => member.Name };

    context.PushGlobal(new ScriptObject { { "Model", model } });
    context.PushCulture(CultureInfo.CurrentCulture);

    return context;
}

public record class Person(string FirstName, string LastName, string City);

public record class Product(string Name, decimal Price, string Description, DateTime Date);

public class PeopleModel
{
    public IEnumerable<Person> People { get; set; } = [];
}

public class ProductsModel
{
    public IEnumerable<Product> Products { get; set; } = [];
}

public class ConfirmationEmailModel
{
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}