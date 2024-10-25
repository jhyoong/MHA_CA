# Backend Take Home Assignment
### Tasks:
1. Create a backend API with 3 endpoints.  
    a. Get an object based on ID from a list of JSON objects.  
    b. Add a new object.  
    c. Delete an object.  
2. Logged down all request to the backend.
3. Request to add and delete object should be validated.
4. Create a couple of unit test cases to test out the endpoints.
5. Implement any form of dependency injection (e.g a simple service to process an added object)

Bonus: Deploy the above app to the cloud
Notes:
1. This should not incur any cost.
2. Database should not be used for this assignment. JSON should be hard coded in a separate file and loaded to memory when application starts.

## Deploying to AWS Cloud
Since I no longer have any free credits on AWS cloud, I'll include in an example cloudformation template to serve this backend API using common AWS cloud services.

[AWS Lambda](https://docs.aws.amazon.com/lambda/latest/dg/csharp-package-asp.html) - some modifications to the code will be required, a little bit of restructuring of the folder structure and addition of the AWS Lambda builder service.

[AWS DynamoDB](https://aws.amazon.com/dynamodb/) - fuss-free simple database that works well with scaling up. However, since there is a mention of not using databases, this will not be used.

[AWS EC2](https://aws.amazon.com/ec2/) - As AWS Lambda has limited runtime (15mins) and will require a separate S3 bucket instance to store the hardcoded JSON object, an EC2 might be a more [straightforward](https://docs.aws.amazon.com/whitepapers/latest/develop-deploy-dotnet-apps-on-aws/running-.net-applications-in-the-aws-cloud.html) choice in hosting this on the cloud. Some references can be found [here](https://aws.amazon.com/blogs/dotnet/net-8-support-on-aws/).

[AWS Gateway](https://aws.amazon.com/api-gateway/) - A core AWS service in managing APIs easily. 

AWS [Cloudwatch](https://aws.amazon.com/cloudwatch/) and [Opensearch](https://aws.amazon.com/opensearch-service/) are great services for setting up logs and monitoring metrics.

### Additional notes
To deploy this template, you'll need to:

1. Have an existing [VPC](https://docs.aws.amazon.com/vpc/latest/userguide/what-is-amazon-vpc.html) and [subnet](https://docs.aws.amazon.com/vpc/latest/userguide/configure-subnets.html)
2. Have an EC2 key pair for SSH access
3. [Deploy](https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/deployment-ecs-aspnetcore-ec2.html) the .NET application to the EC2 instance
4. Update the AMI ID if needed (current template uses Amazon Linux 2023)

# Local runs/tests

## Build the project
```dotnet build```

## Running locally
```dotnet run --project ./IcecreamApi.Api/```

## Run unit tests
```dotnet test```
* Note that if the hardcoded products.json file was modified, it may cause some unit tests to fail. ( which is expected behaviour )
* Run `dotnet clean` after running unit tests.

## Swagger Docs
``` http://127.0.0.1:5277/swagger/index.html ```

## Manual test templates using VS Code REST Client Extension
### GET Ice Cream
```GET http://127.0.0.1:5277/api/products/2 HTTP/1.1```

### POST New Ice Cream
```
POST http://127.0.0.1:5277/api/products/ HTTP/1.1
content-type: application/json

{
    "id": 3,
    "name": "Durian Mao Shan Wang",
    "price": 99.99,
    "category": "Premium"
}
```

### DELETE Ice Cream
```
DELETE http://127.0.0.1:5277/api/products/3 HTTP/1.1
```