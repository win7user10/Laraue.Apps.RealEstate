# Laraue.Apps.RealEstate

Application allows collecting real estate advertisements and ranking them with AI. Advertisements then can be requested 
via API.

### System requirements
1. PostgreSQL database with version 15+. Settings are use default 'postgres:postgres' login/pass as default.
2. Local or remotely available [ollama](https://ollama.com) instance. The system is configured with default Ollama local address,
so after ollama installing application should work without any setup. But the first run will load the model and make takes
a time. The default model "qwen2.5vl:7b" uses near 8GB memory, and preferably it should be GPU memory. 
The model could be changed in settings.

### Crawlers setup
As default, crawlers for Moscow, Saint Petersburg and Volgograd are enabled. It can be changed to the preferable cities.

## Host information
**Laraue.Apps.RealEstate.ApiHost**: Provide API to the ready advertisements.  
**Laraue.Apps.RealEstate.CrawlingHost**: Collect advertisements and put them into DB.  
**Laraue.Apps.RealEstate.GpuWorkerHost**: Sequentially get predictions for each advertisement and update prediction in DB.  
**Laraue.Apps.RealEstate.TelegramHost**: Host to set up telegram notifications about new advertisements.  
**Laraue.Apps.RealEstate.WorkerHost**: The host marking advertisements as ready for API and also remove these flugs.  