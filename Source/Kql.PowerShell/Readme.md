# KQL in PowerShell

This is natural integration of real-time KQL queries into PowerShell.

## User experience

Once the project is build on the local machine, the user starts PowerShell and imports the module:

	import-module kql.powershell

Then, they can see the available commands:

	get-command -module kql.powershell

In the PowerShell environment, natural expectation is that the commands are primitive building blocks. For example, to read windows OS logs the user will do:

	read-winlog Security 

This should read the existing events in the Security log. If instead the user wants to read a local file, the command is:

	read-winlog -File c:\test.evtx

Here `-File` tells us we are reading local files, as opposed to one of the OS or Application and Services logs.

## Applying KQL query on the stream

The PowerShell "pipelines" were designed to automate user's interactions. Like take 100 output row from one command as a variable and pass it to the next.

In contrast, the KQL processing works on streams, which may be high-volume and may be infinite. To apply KQL query to a Windows Log:

	read-winlog -Log Security -Query C:\git\prototypes\WecV4\RxKql\Kql.PowerShell\Filter.kql

Here the stream-processing happens with Rx.KQL. 

Only the output (hopefully small) is surfaced into the PowerShell pipeline.

## Build process

To extend PowerShell, we are:

- building `Kql.PowerShell` as a C# class library
- including the file `DeployModule.cmd` and copying this to the output directory
- running this file using post-build event (see Properties\BuildEvents of the project)


## How to create comand-lets

In PowerShell, the commands conform to a naming convention. For example **Read**-* means we are reading something, **Create**-* is creating new artifacts etc. This makes it easy for users to find the command they need. See the [approved verbs](https://docs.microsoft.com/en-us/powershell/scripting/developer/cmdlet/approved-verbs-for-windows-powershell-commands?view=powershell-7.1).

To create new commands in C#:

- add the NuGet package `Microsoft.PowerShell.5.ReferenceAssemblies`
- add `using System.Management.Automation;`
- inherit from the CmdLet base class
- annotate the classes like `[Cmdlet(VerbsCommunications.Read, "Winlog")]`
- annotate parameters like 

        [Parameter(Mandatory = false, Position = 0)]
        public string Log;

        [Parameter(Mandatory = false)]
        public string File;

Here both `-Log` and `-File` can be passed as explicit parameters. If there are no explicit parameters, PowerShell assumes the user is passing the parameters in positional order such as

	read-winlog Security 

See more [here](https://www.red-gate.com/simple-talk/dotnet/net-development/using-c-to-create-powershell-cmdlets-the-basics/#second)