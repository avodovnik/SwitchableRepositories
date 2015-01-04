SwitchableRepositories
======================

Switchable Repositories is the demo application, belonging to http://www.vodovnik.com/2015/01/02/circuit-breaker-pattern/. For more information, consult the blog post. 

How to run
==========
The app is compilable in Visual Studio, it requires NuGet packages, and are restored immediately. The app tries calling Repository A, if that fails, it uses the circuit breaker pattern to trip. The switchable repository then tries calling Repository B. 
