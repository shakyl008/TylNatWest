# Objectives 
From a first read what I have identified is that the system will need to be fast and manage high volume, it will likely be event driven, security will be important, potentially a central event soruced database could be used - but we need to start simple.

For a simple MVP I designed a system with the following requirements:
#### Receive Trade Notifications in real-time.
-Identify stock has been exchanged using the ticker symbol.

-Record price the stock was traded, denominated in pounds.

-Capturing the number of shares exchanged, which can be a decimal number.

-Store the ID of the broker.

#### Expose Stock Values:
-Allowing retrieval of the latest value of a stock by its ticker.

-Access to the value of all stocks available on the market.

-Provide the values of stocks that match a given list of tickers.


## Assumptions
We are not publishing as soon as a trade happens, only when someone calls the api.

We publish a stock entity which contains the new value of this stock.

We assume that latest sale price is the new value of that stock - this is a very simple assumption, as someone selling a stock for 0.001£ could crash the rest of the system - could extend the system to keep track of total number of stocks sold and thus be able to calculate averages.

The stock and trade entities are distinct.

We assume that StockExchange controls which stock tickers exist and brokers are not allowed to create/introduce new ones

Maybe not all functions should be cached - thinking from a user perspective, if someone asks for the value of a specific stock chances are they want the freshest data (within millisecond accuracy) and caching would affect this freshness.

## Simplifications
Used an in memory database with simple seed data for testing - for the MVP an SQL database should be sufficient

Did not implement a cache, but one would definitely be needed for functions such as GetAllStocksAsync()

The LINQ query GetStocksByTickerListAsync() is sufficient for an MVP, but in high volume system it may be useful to have an index for the ticker column and use a stored procedure 

I did not add authentication directly in MVP to keep the scope limited - planning on having authentication performed at an APImanagement level using another service


# Non Functional Requirements:
#### Availability
During trading hours it will need near 100% availability - maintenance can be performed during out of office hours

#### Platform recovery
It will need to very rapidly recover from outages, frequent backups of stock prices across multiple databases - such that in the event of failure queries can be redirected to the backup database.

#### Regulatory constraints 
Financial transactions usually have audit requirements
Speed and scalability - being able to respond quickly and scale on demand - mainly optimizing the database

#### Observability 
Being able to monitor the system live

#### Security 
Only authorized brokers can access the endpoint; encrypt the data at rest; use https for data in transit

#### Latency 
Can use caching close to the client layer to reduce total travel time; carry out database performance tuning; using asynchronous and parallel processing where beneficial;

#### Load balancing 
LondonStockEchange service would likely need to spawn multiple instances to handle peak load; criteria for creating new instances is likely to be network bandwidth/IO;

#### Extensibility 
The MVP will likely have features added to it so we should structure the code with good OOP principles so it is easy to extend/maintain – eg: layering application, service and repository separately 

#### Data atomicity 
In the mvp in its current form, due to the simplicity of the operations, EntityFramework should by default ensure atomic transactions for writing new stock data but concurrency issues are still likely to occur if multiple instances are writing at the same time.
Load testing - we can setup a staging environment seed data on stocks and trades and try to simulate peak loads

# Instructions provided
Pretend we are the London Stock Exchange and we are creating an API to receive notification of trades from authorised brokers and expose the updated price to them.

We need to receive every exchange of shares happening in real-time, and we need to know:
·What stock has been exchanged (using the ticker symbol)
·At what price (in pound)
·Number of shares (can be a decimal number)
·ID of broker managing the trade

A data store is used to store all transactions happening. Consider a minimal data structure and suggest the technology you prefer.

We need to expose the current value of a stock, values for all the stocks on the market and values for a range of them (as a list of ticker symbols).

Assume you can use SDKs and middleware for common functionalities.

You task is to define a simple version of the API, including a definition of data model. Describe shortly the system design for an MVP.

Enhancements

Is this system scalable? How can it cope with high traffic? Can you identify bottlenecks and suggest an improved design and architecture? Feel free to suggest a complete different approach, but be sure you can obtain the same goal.

Evaluation

This assignment should not take more than 2 hours. We are looking for well structure code and problem solving focus. The solution does not have to be production ready, but consideration of NFRs is important.

The problem definition is quite open, we want to see your ability with wide systems.

About enhancements, we do not expect a complete system design, more a high level description. Prepare your ideas for the upcoming interview!  


