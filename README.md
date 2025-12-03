# HydroMonitor

HydroMonitor is a .NET MAUI Blazor application leveraging MQTTnet to provide a home or office leak detection and environmental monitoring system.

It works in tandem with an MQTT application running on a Raspberry Pi or other device supporting Python to give you real-time insights into the hydrological state of your facility.

## Key Features
- Map feature to easily locate sensors anywhere in your facility.
- Geolocation feature to help guide you to find sensors in difficult to reach locations.
- Blazor Bootstrap Charts to visualize all your sensor data.

### Dependencies:
 - Blazor Bootstrap
 - MQTTNet
 - .NET MAUI

## Installation Instructions
1. Clone master from the Git repository using Visual Studio.
2. Build the code from source using Visual Studio. Ensure you get the appropriate dependencies from NuGet.
3. Run the .exe to launch the app.
4. Configure the Python code to use your PC's IP address as the BROKER_ADDRESS.
5. Run the python code on your device. HydroMonitor will start receiving data.

# Devlog

## Stage Zero - Sensors
Despite not technically being in scope for the project, I did use the knowledge gained from the API class to get a raspberry Pi with a mosquitto broker working for sending data over the network. 


## Stage One - Data
I decided to tackle the start of this project the way any good project is, with the backend. I implemented the SQLite database the same way as was shown in class. The Sensor object was dependent on the SensorType object we tacked on to help the user interface from having to use hardcoded constants. This will make it possible to expand the app to include other types of sensors (for example, leak detection, drywall sensors) in the future without having to add in constants all over the codebase.

The problem with this approach is that the sql-pcl library being used, while quick to add new types of objects, doesn't really deal with relational stuff well. It also doesn't like using GUIDs as primary keys, so we had to pivot and use sequential integer identifiers instead. Whenever I changed the data model, I had to ad-hoc execute a statement to alter the table to include the new fields. Being able to build a database directly from your Model classes is neat, but only really a better choice when you've already got your model figured out.

For the MQTT portion, some preliminary research turned out MQTTNet as the library of choice for doing MQTT client/server stuff in .NET applications. Documentation was not super applicable to the setup that we wanted to run for HydroMon - it took some trial and error to get the right combination of settings for the client to compile and run correctly. 

In particular, a challenge was getting the asynchronous methods to work correctly - since most of the tutorials were for synchronous operation. Having never written async code before, figuring that piece out was a challenge. The problem we experienced was the user interface locking up anytime the service was loaded. Using the "await" keyword and not the .Result property when getting a usable List returned was the key to resolving this issue.

Eventually, the code successfully pulled from the broker on the pi over the network.

So, now the MQTTService could run when a page was loaded and the related code was triggered. But we want the MQTTservice to run as an injected service on startup, and listen for the data coming back from the sensors. This again took more time than anticipated, due to the combination of asynchronous programming and network troubleshooting.

## Stage Two - User Interface

Adding icons by copying the pathing and using find/replace to ensure it matched the format Blazor was using was extremely tedious and time-consuming. I was also finding that a bunch of bootstrap classes were missing from the default project. So, I installed blazor bootstrap.

Well, those five words are doing a lot of heavy lifting. First I tried upgrading the version that was already there, then when that didn't work I tried importing all the icons as a font file. That didn't work either.

Eventually, after coming back to it a few days later, I found that installing the Blazor Bootstrap library from NuGet, and then manually renaming / copying files into the right locations and adding the references to the .csproj file worked. Score. Now we have as powerful UI library that I'm already familiar with to do the user interface.

Styling the components came pretty quickly, but I wanted to add some validation to the pages when the data got submitted. There are packages for doing this in Blazor using some type of form component, but since I had so much trouble getting the async stuff to work, I didn't want to chance rewriting the whole thing again.

I thought it might be cool to have the user interface have a pool / water effect like [that one meme](https://chameleonmemes.com/wp-content/uploads/2024/03/What-is-the-point-fo-this.jpeg) so I searched around online, but didn't really find one. I asked Copilot to generate one for me which had a pulsating gradient effect. The svg filter for a wavy effect it provided didn't work, so I spent an hour or two messing around trying to get the syntax to work. It looked the exact same as other exmaples but seemed to not even register. Maybe MAUI Blazor doesn't support that feature? I gave up on the idea.

To enable the map feature, I had to display an image on the webpage. My first track with this, following with the way the sample code added images, was to add items as a MAUIImage in the Resources folder. This was a complete and utter waste of time. Instead, after speaking with the class professor, I was told I could imply put the images in the wwwroot folder and reference them with the ``src`` tag of an image, like any other webapp. After doing that, the rest of the Map code came together quickly. I designed a component to display and position map markers and was able to reuse it for both the modal picker and the 



## Stage Three - Testing and Security

Testing was, unfortunately, a major hurdle as well. At this point in the project, Microsoft *helpfully* released .NET version 10, which consequently broke everything I was working on in .NET 8. Adding a test project would mysterioudly not recognize references to the main project, leaving me with no Intellisense at all. It also refused to compile, since the EOL version errors registered as compilation problems.

Fixing the issue with the test project required upgrading to .NET 9.0, adding a reference to native .net9.0 to the .csproj file, and disabling the exe compilation target. It sounds simple, but finding that combination of steps took well over eight hours - and I only uncovered the fix after limiting the search to the previous month and putting the right error message into the searchbar.

Once the tests compiled, I was able to write a test for the ARPService to test that it was returning the MAC address in the correct format. Turns out, it wasn't! So, writing the tests was actually useful to catch this error.

Testing the MQTTService proved a little more challenging. The test project didn't have any of the dependencies of the main project, so things like sqlite-net-pcl and mqttnet were unavailable. Mocking the DAO objects wasn't too difficult once I found the right pattern, but I got lost trying to mock the MQTT stuff. Since we didn't really cover mocking in the class, I could only go as far as testing anything which didn't have dependencies. Testing the Blazor components themselves was pretty much out of the question.

I spent the final day of the project prior to the second checkin  implementing a rudimentary security feature. While the MAC address of the sender isn't provided when an MQTT client tries to connect, the IP address is. And since we have the ARPService in place, we can check that the MAC address of the client actually matches with the /setup link they're requesting.


## Stage Four - Android Native Features

The geocoding stuff was not difficult to implement into the code, thanks to Microsoft's MAUI libraries. Verifying that any of it worked at all, though, was a lot less doable. It took an hour and a half of fiddling about just to get debugging to work. Because I was doing all the development from a virtual machine, the USB debugging didn't work, and I had to get the networking stuff going. 

```adb connect``` didn't work, but ```adb pair``` did... the second time I tried it. Whatever, I'm not asking questions. My app was on my phone and I could test it to my hearts content. I was able to refabulate the geocoding feature to actually do something approximating useful.


