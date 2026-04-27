# AWS Lambda Integration Guide

## ?? Overview

Your application now seamlessly integrates **local debugging** with **AWS Lambda deployment** using the same codebase and routing logic. The application automatically detects its environment and runs in the appropriate mode.

## ?? How It Works

### Environment Detection

```csharp
var isRunningInLambda = !string.IsNullOrEmpty(
    Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")
);
```

- **Local Mode**: When `AWS_LAMBDA_FUNCTION_NAME` is not set ? Runs HTTP server with Swagger UI
- **Lambda Mode**: When environment variable is present ? Runs as Lambda function

### Architecture Flow

```
???????????????????????????????????????????????????????????????
?                     Program.cs Entry Point                  ?
???????????????????????????????????????????????????????????????
?  1. Setup DI Container                                      ?
?  2. Register Mappers, Mediators, Controllers                ?
?  3. Detect Environment (Local vs Lambda)                    ?
???????????????????????????????????????????????????????????????
                   ?
        ???????????????????????
        ?                     ?
        ?                     ?
?????????????????    ??????????????????????
?  LOCAL MODE   ?    ?   LAMBDA MODE      ?
?????????????????    ??????????????????????
? HTTP Server   ?    ? API Gateway        ?
? Port: 5100    ?    ? Lambda Handler     ?
? Swagger UI    ?    ?                    ?
?????????????????    ??????????????????????
        ?                      ?
        ????????????????????????
                   ?
                   ?
        ????????????????????????
        ? GeneratedApiRouter   ?
        ? - Match Route        ?
        ? - Extract Params     ?
        ? - Call Controller    ?
        ? - Return Result      ?
        ????????????????????????
```

## ?? Local Debugging Mode

### How to Run Locally

```bash
dotnet run --project AotSample
```

### What Happens

1. ? **HTTP server starts** on `http://localhost:5100`
2. ? **Swagger UI** available at `/swagger`
3. ? **API endpoints** accessible directly
4. ? **Full development experience** with hot reload

### Console Output

```
===========================
? Server is running!
?? Open your browser and navigate to:
   ?? Swagger UI: http://localhost:5100/swagger
   ?? API Endpoints JSON: http://localhost:5100/swagger/api/endpoints.json

Press any key to stop the server...
```

### Test Endpoints Locally

```bash
# Get users
curl http://localhost:5100/api/users

# Get specific user
curl http://localhost:5100/api/users/123

# Create user
curl -X POST http://localhost:5100/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com"}'

# Create product
curl -X POST http://localhost:5100/api/products \
  -H "Content-Type: application/json" \
  -d '{"command":{"name":"Widget","price":19.99}}'
```

## ?? AWS Lambda Deployment Mode

### Lambda Handler Function

The `OrchestrateAsync` function handles all API Gateway requests:

```csharp
static async Task<APIGatewayProxyResponse> OrchestrateAsync(
    APIGatewayProxyRequest apiRequest, 
    ILambdaContext context,
    IServiceProvider serviceProvider)
{
    // 1. Extract request information
    var path = apiRequest.Path ?? "/";
    var method = apiRequest.HttpMethod ?? "GET";
    var routeValues = new Dictionary<string, string>(apiRequest.PathParameters ?? new());
    var queryValues = new Dictionary<string, string>(apiRequest.QueryStringParameters ?? new());
    var body = apiRequest.Body;

    // 2. Route to appropriate controller endpoint
    var result = await GeneratedApiRouter.RouteAsync(
        path, method, routeValues, queryValues, body, 
        serviceProvider, CancellationToken.None
    );

    // 3. Convert to API Gateway response
    return new APIGatewayProxyResponse
    {
        StatusCode = result.StatusCode,
        Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Access-Control-Allow-Origin", "*" },
            { "Access-Control-Allow-Headers", "Content-Type,Authorization" },
            { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,PATCH,OPTIONS" }
        },
        Body = SerializeResponse(result)
    };
}
```

### Request Flow in Lambda

```
API Gateway Request
    ?
???????????????????????????????????????
? APIGatewayProxyRequest              ?
???????????????????????????????????????
? • Path: /api/users/123              ?
? • HttpMethod: GET                   ?
? • PathParameters: { id: "123" }     ?
? • QueryStringParameters: { }        ?
? • Body: null                        ?
? • Headers: { ... }                  ?
???????????????????????????????????????
              ?
    OrchestrateAsync
              ?
    Extract & Parse
    • path = "/api/users/123"
    • method = "GET"
    • routeValues = { "id": "123" }
    • queryValues = { }
              ?
    GeneratedApiRouter.RouteAsync
              ?
    Match Route Pattern
    • Pattern: "/api/users/{id}"
    • Matched: ?
    • Extract: id = "123"
              ?
    Call Controller Method
    • controller.GetUser(id: "123")
              ?
    Return RouteResult
    • StatusCode: 200
    • Data: UserModel { ... }
              ?
???????????????????????????????????????
? APIGatewayProxyResponse             ?
???????????????????????????????????????
? • StatusCode: 200                   ?
? • Headers: { Content-Type: ... }    ?
? • Body: "{ 'id':'123', ... }"       ?
???????????????????????????????????????
              ?
    API Gateway Response
```

## ??? How Routing Works

### 1. Path Matching

```csharp
// Pattern: /api/users/{id}
// Actual:  /api/users/123

if (MatchesRoute(path, "/api/users/{id}", out var routeParams))
{
    // routeParams = { "id": "123" }
    var id = routeParams["id"]; // "123"
}
```

### 2. HTTP Method Matching

```csharp
if (method == "GET" && MatchesRoute(path, pattern, out var params))
{
    // Handle GET request
}
else if (method == "POST" && path == "/api/users")
{
    // Handle POST request
}
```

### 3. Parameter Extraction

#### Route Parameters (Path Variables)
```csharp
// API Gateway: pathParameters: { "id": "123" }
var id = routeParams["id"]; // "123"
```

#### Query Parameters
```csharp
// API Gateway: queryStringParameters: { "page": "2", "size": "10" }
var page = queryValues.TryGetValue("page", out var pageValue) 
    ? int.Parse(pageValue) 
    : 1;
```

#### Body Parameters
```csharp
// API Gateway: body: "{ \"name\": \"John\" }"
var command = JsonSerializer.Deserialize<CreateUserCommand>(
    body ?? "{}", 
    AotJsonContext.Default
);
```

## ?? Deployment to AWS Lambda

### 1. Build for Lambda

```bash
# Build as self-contained
dotnet publish -c Release -r linux-x64 --self-contained

# Or with AOT (faster cold starts)
dotnet publish -c Release -r linux-x64 /p:PublishAot=true
```

### 2. Package Lambda Function

```bash
cd AotSample/bin/Release/net8.0/linux-x64/publish
zip -r ../function.zip .
```

### 3. Deploy to AWS Lambda

#### Using AWS CLI
```bash
aws lambda create-function \
  --function-name ZeroReflectionApi \
  --runtime provided.al2 \
  --handler bootstrap \
  --zip-file fileb://function.zip \
  --role arn:aws:iam::YOUR_ACCOUNT:role/lambda-execution-role \
  --environment Variables={ASPNETCORE_ENVIRONMENT=Production} \
  --timeout 30 \
  --memory-size 512
```

#### Using AWS SAM Template

```yaml
AWSTemplateFormatVersion: '2010-09-09'
Transform: AWS::Serverless-2016-10-31

Resources:
  ZeroReflectionApi:
    Type: AWS::Serverless::Function
    Properties:
      CodeUri: AotSample/
      Handler: bootstrap
      Runtime: provided.al2
      Architectures:
        - x86_64
      MemorySize: 512
      Timeout: 30
      Events:
        ApiEvent:
          Type: HttpApi
          Properties:
            Path: /{proxy+}
            Method: ANY
```

### 4. Create API Gateway

```bash
aws apigatewayv2 create-api \
  --name ZeroReflectionApi \
  --protocol-type HTTP \
  --target arn:aws:lambda:region:account-id:function:ZeroReflectionApi
```

## ?? Configuration

### Environment Variables in Lambda

Set these in Lambda configuration:

```bash
ASPNETCORE_ENVIRONMENT=Production
AWS_LAMBDA_FUNCTION_NAME=ZeroReflectionApi  # Auto-set by Lambda
```

### CORS Configuration

CORS headers are automatically included in responses:

```csharp
Headers = new Dictionary<string, string>
{
    { "Access-Control-Allow-Origin", "*" },
    { "Access-Control-Allow-Headers", "Content-Type,Authorization" },
    { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,PATCH,OPTIONS" }
}
```

For production, restrict `Access-Control-Allow-Origin` to your domain:

```csharp
{ "Access-Control-Allow-Origin", "https://yourdomain.com" }
```

## ?? Endpoint Mapping Examples

### Example 1: GET with Route Parameter

**API Gateway Request:**
```json
{
  "path": "/api/users/123",
  "httpMethod": "GET",
  "pathParameters": {
    "id": "123"
  }
}
```

**Routing:**
```
Pattern: /api/users/{id}
Match: ?
Extract: id = "123"
Call: UsersController.GetUser("123")
```

**Response:**
```json
{
  "statusCode": 200,
  "body": "{\"id\":\"123\",\"name\":\"John Doe\",\"email\":\"john@example.com\"}"
}
```

### Example 2: POST with Body

**API Gateway Request:**
```json
{
  "path": "/api/users",
  "httpMethod": "POST",
  "body": "{\"name\":\"Jane Doe\",\"email\":\"jane@example.com\"}"
}
```

**Routing:**
```
Pattern: /api/users
Match: ?
Deserialize: CreateUserCommand from body
Call: UsersController.CreateUser(command)
```

**Response:**
```json
{
  "statusCode": 200,
  "body": "{}"
}
```

### Example 3: GET with Query Parameters

**API Gateway Request:**
```json
{
  "path": "/api/users",
  "httpMethod": "GET",
  "queryStringParameters": {
    "page": "2",
    "pageSize": "10"
  }
}
```

**Routing:**
```
Pattern: /api/users
Match: ?
Extract: page = 2, pageSize = 10
Call: UsersController.ListUsers(page, pageSize)
```

## ?? Performance Benefits

### Cold Start Optimization

With **Native AOT** compilation:
- ? **~100ms cold starts** (vs ~2s with JIT)
- ? **Smaller package size** (~10MB vs ~60MB)
- ? **Lower memory usage**

### Runtime Performance

Using **source generators**:
- ? **Zero reflection** - All routing compiled
- ? **Fast dispatch** - Direct method calls
- ? **Type-safe** - Compile-time validation

## ?? Testing

### Local Testing

```bash
# Start local server
dotnet run

# Test with curl
curl http://localhost:5100/api/users
```

### Lambda Testing with SAM

```bash
# Install SAM CLI
sam local start-api

# Test endpoint
curl http://localhost:3000/api/users
```

### Lambda Testing with AWS CLI

```bash
aws lambda invoke \
  --function-name ZeroReflectionApi \
  --payload '{"path":"/api/users","httpMethod":"GET"}' \
  response.json

cat response.json
```

## ?? Debugging Lambda Issues

### Enable Logging

```csharp
context.Logger.LogInformation($"Processing {method} request to {path}");
context.Logger.LogInformation($"Route values: {JsonSerializer.Serialize(routeValues)}");
context.Logger.LogInformation($"Query values: {JsonSerializer.Serialize(queryValues)}");
```

### Check CloudWatch Logs

```bash
aws logs tail /aws/lambda/ZeroReflectionApi --follow
```

### Common Issues

#### 1. Route Not Matched
```
Problem: 404 Not Found
Solution: Check path pattern in GeneratedApiRouter
```

#### 2. Deserialization Error
```
Problem: 400 Bad Request
Solution: Ensure AotJsonContext includes all DTOs
```

#### 3. Missing Environment Variable
```
Problem: Application runs in wrong mode
Solution: Verify AWS_LAMBDA_FUNCTION_NAME is set
```

## ?? Summary

### ? What You Have Now

1. **Unified Codebase** - Same routing logic for local and Lambda
2. **Environment Detection** - Automatic mode selection
3. **Full Integration** - API Gateway ? Lambda ? Controllers
4. **Type Safety** - Compile-time validation with source generators
5. **Performance** - Zero reflection, AOT-compatible
6. **Developer Experience** - Local Swagger UI for testing

### ?? Deployment Checklist

- [ ] Build project with `dotnet publish`
- [ ] Package as zip file
- [ ] Create Lambda function in AWS
- [ ] Set up API Gateway integration
- [ ] Configure CORS headers
- [ ] Test endpoints
- [ ] Monitor CloudWatch logs

### ?? Next Steps

1. **Add Authentication**: Integrate JWT/API Key validation
2. **Add Monitoring**: CloudWatch metrics and traces
3. **Add Caching**: DynamoDB or ElastiCache
4. **Add API Versioning**: Support multiple API versions
5. **Add Rate Limiting**: Protect against abuse

**Your application is now ready for both local development and AWS Lambda deployment!** ??
