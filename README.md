# Instructions for candidates

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/) on the details of this exercise, please do so now. 

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

Feel free to change the structure of the solution, use a different test library etc.


## Assumptions
- Dependancies such as dotnet, aspnetcore-runtime, dotnet-core are already installed - this should be a step in the pipeline
- Currencies of USD, EUR, GBP are acceptable, they are likely to be the most commonly used
- It is ok to add some commonly used NuGet packages (e.g. FluentAssertions, Moq)
- Validating amount in the request is not specified, but I will treat a value less than 1 as invalid
- Declined payments are still persisted, rejected payments (due to a bad request) are not
- Running integration tests while debugging requires a running mountebank container, otherwise they will fail

## Functional Requirements
- POST request to submit a payment
    - Request body should include json like
        {
            cardNumber: string,
            expiryMonth: short,
            expiryYear: short,
            currency: string,
            amount: int,
            cvv: string
        }
    - Perform validation on:
        - cardNumber - regex check for numeric chars only, length must be between 14 and 19 chars
        - expiryMonth - cast to short, between 1 and 12 inclusive?
        - expiryYear - once concatenated with expiryMonth, it must be in the future - compare to Date.now().format('mm-yyyy')
        - currency - lets just compare this value to ('USD', 'EUR', 'GBP'), reject if not one of these
        - amount - should not be negative
        - cvv - ensure it is only 3 or 4 chars
    - Successful response will be something like
        {
            id: guid,
            status: enum,
            lastFourDigits: string,
            expiryMonth: short,
            expiryYear: short,
            currency: enum,
            amount: int
        }
- GET request to fetch a previous payment:
    - path param of {id: guid}
    - Successful response will be something like
        {
            id: guid,
            status: enum,
            lastFourDigits: string,
            expiryMonth: short,
            expiryYear: short,
            currency: enum,
            amount: int
        }

## Non functional requirements
- 100% unit test coverage
- Happy/unhappy path integration tests
    - Can I make a successful POST and then GET it?
    - Can I make an unsuccessful POST and then have it not exist in DB?
