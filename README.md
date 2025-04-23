# IP Broker Service


The IP Broker Service is a program that demonstrates the use of multiple IP geolocation providers to retrieve location information for a given IP address. It showcases the distribution of requests among different providers, error handling, rate limit management, and error recovery.
# Features
-	Provider Configuration: The program allows you to configure multiple IP geolocation providers with different characteristics such as rate limits, artificial delays, and error simulation rates.
-	Initial Distribution: The program runs a set of requests to show the initial distribution of requests among the configured providers and demonstrates error handling.
-	Rate Limit & Error Recovery: The program runs additional requests to demonstrate how the system recovers and rebalances the requests when providers reach their rate limits or encounter errors.
-	Statistics: The program displays provider statistics, including the number of requests in the last minute, successful requests, error rate, and average response time.
# Getting Started
To get started with the IP Broker Service, follow these steps:
1.	Clone the repository to your local machine.
2.	Open the solution in Visual Studio.
3.	Build the solution to restore dependencies and compile the code.
4.	Run the program by executing the Main method in the Program class.
# Configuration
The IP Broker Service allows you to configure the IP geolocation providers in the Program.cs file. In the Main method, you can find the providers list, where you can add or modify the providers according to your requirements. Each provider is represented by an instance of the ProviderStats class, which takes the following parameters:
-	ProviderName: The name of the provider.
-	BaseUrl: The base URL of the provider's API.
-	RateLimit: The rate limit for the provider.
-	EndpointFormat: The format of the API endpoint for retrieving location information. Use {0} as a placeholder for the IP address.
-	ArtificialDelay: The artificial delay to affect the selection priority of the provider.
-	SimulateErrorRate: The rate at which the provider should simulate errors.

# Usage
When you run the IP Broker Service, it will go through two phases:
1.	Phase 1: Initial Distribution: This phase demonstrates the initial distribution of requests among the configured providers. It runs a set of requests to retrieve location information for a specific IP address. The program will display the provider selection and any encountered errors.
2.	Phase 2: Rate Limit & Error Recovery: This phase demonstrates how the system recovers and rebalances the requests when providers reach their rate limits or encounter errors. It runs additional requests and displays the provider statistics, including the number of requests, successful requests, error rate, and average response time.

# Noteworthy Points
1. Multiple providers management
   - Handles multiple geolocation providers with different characteristics (Like different error rates)
   - Implements provider-specific configuration for flexibility
2. Rate Limit Compliance
   - Respects configured rate limits for each provider(Rate limits are part of arguments as part of demonstration)
   - Implements tracking mechanisms to prevent exceeding limits
3. Proper Monitoring
   - Provides detailed statistics on provider performance
   - Tracks key metrics including success rates, error rates, and response times
   - Visualizes performance data for easy interpretation


