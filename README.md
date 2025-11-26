# HydroMonitor

HydroMonitor is a .NET MAUI Blazor application leveraging MQTTnet to provide a home or office leak detection and environmental monitoring system.

Dependencies:
 - Blazor Bootstrap
 - MQTTNet
 - .NET MAUI



#Devlog

##Stage Zero - Sensors
Despite not technically being in scope for the project, I did use the knowledge gained from the API class to get a raspberry Pi with a mosquitto broker working for sending data over the network. 


##Stage One - Data
I decided to tackle the start of this project the way any good project is, with the backend. I implemented the SQLite database the same way as was shown in class. The Sensor object was dependent on the SensorType object we tacked on to help the user interface from having to use hardcoded constants. This will make it possible to expand the app to include other types of sensors (for example, leak detection, drywall sensors) in the future without having to add in constants all over the codebase.

The problem with this approach is that the sql-pcl library being used, while quick to add new types of objects, doesn't really deal with relational stuff well. It also doesn't like using GUIDs as primary keys, so we had to pivot and use sequential integer identifiers instead.

For the MQTT portion, some preliminary research turned out MQTTNet as the library of choice for doing MQTT client/server stuff in .NET applications. Documentation was not super applicable to the setup that we wanted to run for HydroMon - it took some trial and error to get the right combination of settings for the client to compile and run correctly. 

In particular, a challenge was getting the asynchronous methods to work correctly - since most of the tutorials were for synchronous operation. Having never written async code before, figuring that piece out was a challenge. The problem we experienced was the user interface locking up anytime the service was loaded. Using the "await" keyword and not the .Result property when getting a usable List returned was the key to resolving this issue.

Eventually, the code successfully pulled from the broker on the pi over the network.

So, now the MQTTService could run when a page was loaded and the related code was triggered. But we want the MQTTservice to run as an injected service on startup, and listen for the data coming With the MQTTService written, the next challenge was getting it to run as an injected service on startup. This again took more time than anticipated, due to the combination of asynchronous programming

## Stage Two - User Interface

Adding icons by copying the pathing and using find/replace to ensure it matched the format Blazor was using was extremely tedious and time-consuming. I was also finding that a bunch of bootstrap classes were missing from the default project. So, I installed blazor bootstrap.

Well, those five words are doing a lot of heavy lifting. First I tried upgrading the version that was already there, then when that didn't work I tried importing all the icons as a font file. That didn't work either.

Eventually, after coming back to it a few days later, I found that installing the Blazor Bootstrap library from NuGet, and then manually renaming / copying files into the right locations and adding the references to the .csproj file worked. Score. Now we have as powerful UI library that I'm already familiar with to do the user interface.

Styling the components came pretty quickly, but I wanted to add some validation to the pages when the data got submitted. There are packages for doing this in Blazor using some type of form component, but since I had so much trouble getting the async stuff to work, I didn't want to chance rewriting the whole thing again.

I thought it might be cool to have the user interface have a pool / water effect like [that one meme](https://chameleonmemes.com/wp-content/uploads/2024/03/What-is-the-point-fo-this.jpeg) so I searched around online, but didn't really find one. I asked Copilot to generate one for me which had a pulsating gradient effect. The svg filter for a wavy effect it provided didn't work, so I spent an hour or two messing around trying to get the syntax to work. It looked the exact same as other exmaples but seemed to not even register. Maybe MAUI Blazor doesn't support that feature? I gave up on the idea.


## Stage Three - Testing and Security

To be continued...
