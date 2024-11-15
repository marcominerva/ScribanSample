using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Scriban;
using Scriban.Runtime;

namespace ScribanSample;

public static class OrderConfirmation
{
    public static async Task<string> CreateOrderAsync()
    {
        var template = Template.Parse("""
        <html>
            <head>
                <style media="all" type="text/css">
                html {
                    font-size: 16px;
                    font-family: Helvetica Neue, Helvetica, Arial, and sans-serif;
                }
                table {
                        border-collapse: collapse;
                        width: 100%;
                    }
                    th, td {
                        font-size: 16px;
                        padding: 8px;
                        text-align: left;
                    }
                    th {
                        background-color: green;
                        color: white;
                    }
                    .right { 
                        text-align: right; 
                    }
                    .even {
                        background-color: #f2f2f2;
                    }
                    .odd {
                        background-color: #ffffff;
                    }
                </style>
            </head>
            <body>
                <p>
                    Dear {{ Model.CustomerName }},
                    <br />
                    Here it is the details of the order you placed on {{ Model.Date | date.to_string '%d/%m/%Y %R' }}.
                </p>
                <table>
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Description</th>
                            <th class="right">Price</th>
                            <th class="right">Quantity</th>
                            <th class="right">Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        {{- for product in Model.Products }}
                            <tr class="{{ if for.even }}even{{ else }}odd{{ end }}">
                                <td>{{ product.Name }}</td>
                                <td>{{ product.Description }}</td>
                                <td class="right">{{ product.UnitPrice | object.format "C" }}</td>
                                <td class="right">{{ product.Quantity }}</td>
                                <td class="right">{{ product.TotalPrice | object.format "C" }}</td>
                            </tr>
                        {{- end }}
                    </tbody>
                </table>
                <p align="right">Total: {{ Model.Total | object.format "C" }}</p>
            </body>
        </html>
        """);

        var productFaker = new Faker<Product>();
        productFaker.RuleFor(p => p.Name, f => f.Commerce.ProductName());
        productFaker.RuleFor(p => p.Description, f => f.Commerce.ProductDescription());
        productFaker.RuleFor(p => p.UnitPrice, f => f.Random.Decimal(1, 100));
        productFaker.RuleFor(p => p.Quantity, f => f.Random.Number(1, 10));

        var order = new Order
        {
            CustomerName = "Marco",
            Date = DateTime.Now,
            Products = productFaker.Generate(7)
        };

        var context = new TemplateContext { MemberRenamer = member => member.Name };
        context.PushGlobal(new ScriptObject { { "Model", order } });
        context.PushCulture(CultureInfo.CurrentCulture);

        var result = await template.RenderAsync(context);
        return result;
    }
}

public class Order
{
    public string? CustomerName { get; set; }

    public DateTime Date { get; set; }

    public IList<Product> Products { get; set; } = [];

    public decimal Total => Products?.Sum(p => p.TotalPrice) ?? 0;
}

public class Product
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice => UnitPrice * Quantity;
}