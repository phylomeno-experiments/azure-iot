# azure-iot

## Setup Azure IoT Hub

```
az login
az group create --name iotproject --location northeurope
az iot hub create --name iotproject-phylomeno --resource-group iotproject --sku S1
```

## Query connection strings
```
az iot hub policy show --name service --query primaryKey --hub-name phylomeno-hub1
```
