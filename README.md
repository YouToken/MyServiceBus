# MyServiceBus

**How to start service in docker:**

```
 docker run --env MessagesConnectionString="CONNECTION_STRING_TO_AZURE_STORAGE" --env QueuesConnectionString="CONNECTION_STRING_TO_AZURE_STORAGE" -p 6421:6421 -p 6123:6123 -p 6124:6124 myjettools/my-service-bus:VERSION
```

* CONNECTION_STRING_TO_AZURE_STORAGE - connection string to Azure strorage to use blobs to keep data
* VERSION - version of service image
