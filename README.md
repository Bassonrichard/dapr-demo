# DarpDemo
-------
Demo repo to showcase some darp functionality 
more information can be found on : https://docs.dapr.io/

## Quick Installation Guide

* Install Dapr Cli
    `https://docs.dapr.io/getting-started/install-dapr-cli/`
   
* Dapr init

   Run 
   
   ```
   dapr init
   ```
   
 * Dapr debug sidecar
    
    Run the sidecar for the `daprtestclient`:
    
    ```
     daprd --app-id daprtestclient --components-path ../components/  --metrics-port 9091
    ```
    
        
    Run the sidecar for the `daprtestserver`:
    
    ```
     daprd --app-id daprtestserver --components-path ../components/ --app-port 5008 --dapr-grpc-port 50002 --dapr-http-port 3501
    ```

You can now after following the above steps rung the services in debug mode.

## Service flow

### redis
  * daprtestclient[GetWeatherForecast] -> dapr ->  redis -> dapr -> daprtestserver[weatherforecast]
  * daprtestclient[PostWeatherForecast] -> dapr ->redis
  * daprtestclient[PublishWeatherForecast] -> ->redis -> dapr -> daprtestserver[forecast]

![Dapr overview](./docs/service_flow_redis.png)

