# Nucleus Coop Contributing Guide  
Want to help out with the project? We'd love it! There are lots of different ways that you can help, whether it is developing game handlers, contributing to the project codebase, or simply a donation - all are appreciated.

## Join Discord!
The "developers" channel on the [official Discord Server](https://discord.gg/QDUt8HpCvr) is a great place to get started! Introduce yourself! Chat with the current project developers about what you would like to work on! This is a great way to get to know those working on the project and can also help guide your development contributions, coordinate work better, etc. 

You can also find many of us in the Splitscreen Dreams Discord server, a special community created specifically to make games playable in split-screen (invite only).

## Game Handlers
If you’d like to expand the list of supported games (or improve an existing game’s support), check out the detailed [Handler Development Guide](https://www.splitscreen.me/docs/create-handlers)! We want to make split-screen available for even more games, and we want them to be available for everyone! Creating and sharing handlers is what makes that happen!

## Issue Contribution
If you encounter an issue while using Nucleus Coop, or have an idea for a new feature, you can [create an issue](https://github.com/SplitScreen-Me/splitscreenme-nucleus/issues/new/choose). This will take you to a selection page where you can choose the most relevant template for your issue. We are always open to suggestions!

The SplitScreen.Me FAQ has a great article that discusses [Bug Reporting](https://www.splitscreen.me/docs/faq/#18--where-can-i-report-a-bugissue) in more detail, including how to enable and view log files.

If you want to suggest a new feature or report a bug for any of our _other_ projects (for example our discord bot, or websites), please check out the [appropriate repositories](https://github.com/orgs/SplitScreen-Me/repositories).

## Security Note
If you identify a security issue in the code of Nucleus Coop, please do not post a public issue identifying the security threat. This potentially highlights the issue for malicious users to take advantage of the vulnerability. 
Instead, please ping @Administrators, @Moderators, or @Developers in the support channel of the Nucleus Coop [Discord server](https://discord.com/invite/qnbZtpECYg), and only share the details once a secure channel of communication has been established.

Alternatively you can [create a repository security advisory](https://docs.github.com/en/code-security/security-advisories/working-with-repository-security-advisories/creating-a-repository-security-advisory)

## Working on the Source Code
If you would like to contribute to the codebase of Nucleus Coop as a developer, we would love to have you! Here are some helpful guidelines to make the process go as smoothly as possible!

### The Process
1.	**Fork the master branch** and clone it into your own development environment. As of right now, the “master” branch is the primary, active branch of the project.

2.	**Work on your changes**. Make sure to check for updates to the master branch before you create a pull request to avoid unnecessary code conflicts.

3.	 [**Create a pull request**](https://github.com/SplitScreen-Me/splitscreenme-nucleus/pulls) once you are done with your changes! Please verify that your fork is caught up with the master branch beforehand.

4.	**Your code will be reviewed**, and changes may be requested. Address any issues and repeat as necessary until your change is accepted!

5.	**Done!** Once your code is reviewed and accepted, it will be merged into the codebase of the project! Congratulations! You should see your changes in the next release build!

Check out the official GitHub articles to learn more about [forking](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/fork-a-repo), [cloning](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository), and [pull requests](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request).

### Getting Started

#### What is the Project's Tech Stack?
Nucleus Coop is built for modern Windows PC's and is primarily developed in C# using the .NET Framework, with a _tiny_ portion of the codebase being in C++ or other languages. It also incorporates a variety of open-source tools and libraries to deliver its split-screen PC gaming experience. Several of these dependencies are coded in other languages, such as C and C++ (while some others like Proto Input are largely developed in C# too).

Proto Input and x360ce are the most substantial dependencies, as they are responsible for handling controller assignments and hooks, and virtualizing controllers (respectively). 

#### Required Software Tools
* [Microsoft Visual Studio](https://visualstudio.microsoft.com/)*
* [Visual Studio SDK](https://learn.microsoft.com/en-us/visualstudio/extensibility/installing-the-visual-studio-sdk)
* [Microsoft Visual C++ 2015-2019 Redistributable x86](https://aka.ms/vs/17/release/vc_redist.x86.exe) (or newer)
* [Microsoft Visual C++ 2015-2019 Redistributable x64](https://aka.ms/vs/17/release/vc_redist.x64.exe) (or newer)
* [Git](https://www.git-scm.com/)
* [.NET SDK](https://dotnet.microsoft.com/en-us/download) (4.7.2 or higher)
* IDE of your choice (if not developing in Visual Studio)

All of these tools must be correctly installed, or you will have trouble compiling/running code or setting up your development environment.

*Note: you will need full-fledged Visual Studio, not just Visual Studio _Code_ with C# plugins, as you will need to compile source code from .sln files from within Visual Studio itself as part of creating builds.

#### Submodule Dependencies
These will be taken care of as part of the setup git commands, but for reference purposes, these are the submodule dependencies that Nucleus Coop uses (and several of these have their own sub-dependencies, which you can explore on their respective repositories).
* [Proto Input](https://github.com/Ilyaki/ProtoInput) - Libraries that handle input redirects and hook configuration
* [x360ce](https://github.com/x360ce/x360ce) - X-Box 360 controller emulator
* [TypescriptSyntaxPaste](https://github.com/nhabuiduc/TypescriptSyntaxPaste) -  Converts C# syntax to typescript
* [nukeupdater](https://github.com/lucasassislar/nukeupdater) - Auto-updates

#### Local Development Environment Setup 
1. Fork the repo on GitHub.
2. Clone the repo:

     `git clone https://github.com/JoeyJoeJoeShabadooFork/splitscreenme-nucleus`

3. Navigate to the root folder and update the submodules to make sure that all submodule dependencies are downloaded and up-to-date by running the following command:
     ```sh
     cd splitscreenme-nucleus
     git submodule update --init --recursive
     ```
4. Install (or Build) Proto Input
  
   **Option 1 (Recommended):** Use Post-Compiled Proto Input Files

   
   This is the simplest approach to handling Proto Input file dependencies. If you have no plans to work with the Proto Input source code directly, this is the recommended approach. It is much simpler and has less potential for complications.


   First, download the most recent version of Nucleus Coop from [Releases](https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases). Next you will want to compile Nucleus Coop itself following the directions in the following section. Last, copy and paste the following files from the root folder of the release build download's root into the root directory of your new Nucleus Coop build (the "Release" or "Debug" subfolder contained within "\splitscreenme-nucleus\Master\NucleusCoopTool\bin\"):
     * ProtoInputHooks32.dll
     * ProtoInputHooks64.dll
     * ProtoInputHost.exe
     * ProtoInputIJ32.exe
     * ProtoInputIJ64.exe
     * ProtoInputIJP32.dll
     * ProtoInputIJP64.dll
     * ProtoInputLoader32.dll
     * ProtoInputLoader64.dll
     * ProtoInputUtilDynamic32.dll
     * ProtoInputUtilDynamic64.dll

   This ensures that the files that would have been built by compiling Proto Input are placed in the correct location in the Nucleus Coop build folder with the executable.
   
   **Option 2:** Batch Build **Proto Input** by opening its solution file in Visual Studio and batch building it ("Submodules\ProtoInput\src\ProtoInput\ProtoInput.sln" within the project root).
  
     The cleanest way to do this is probably to sort by "Solution Config" header under the Batch Build menu and check the boxes that correspond with the version you would like to compile (make sure to check both x86 and x64 boxes for your selected build type).

     Check out the troubleshooting section for additional tips on getting Proto Input to compile correctly, if you have any problems! Proto Input appears to be very sensitive to your development environment, so you will likely need to consult this section, especially if you are trying to work with it within Visual Studio 2022.

     Close the Proto Input project out when you're done.

You are now free to work on the Nucleus Coop source code - all dependencies in place! If you get any errors in compiling along the way, you may need to install and run project solution files from an earlier version of Visual Studio or the .NET SDK for better project compatibility.

### Compiling and Testing Changes

When you are ready to test any changes you make, compile **Nucleus Coop** by opening its solution file and running a batch build on it within Visual Studio ("Master\NucleusCoop.sln"). 

> [!IMPORTANT]  
> Your project path should not contain folders with spaces in their name, otherwise Nucleus Coop may not compile correctly.

Similar to Proto Input, on the Batch Build menu, select all the x86 and x64 options for the build you would like to compile. 

Once you have selected your build options and ran the batch build command, your output build directory should have a fully functioning version of the program and everything it needs to run! You can now go to the build directory and run "NucleusCoop.exe" to test out your new build.

As a reminder, please take care to follow the [Code Standards](#code-standards) for the project when adding any code-based contributions.

Test your changes **thoroughly** before submitting a Pull Request. Whenever appropriate, it is strongly recommended that you develop unit-tests with a good test spread to make sure that your new features are working as intended and are capable of handling anticipated edge cases. In cases where this is not possible, please be sure to follow a similar testing paradigm where both average and edge use cases are tested. In the long run, this will save the project a lot of time, avoiding tedious backtracking and bug-smashing by proactively ensuring that easily preventable bugs don't sneak into the master build.

#### Build Configurations
* Release - The release version of the app (live, user-facing build)
* Debug - Debug version of the app (extra debug features enabled)

### Submitting Changes

#### Create a Pull Request
Once you have finished building and testing your changes, it's time to submit them! From the /SplitScreen-Me/splitscreenme-nucleus repository, go to the Pull Requests section and click the "New Pull Request" button.

**Make sure that the _base_ repository is set to "/SplitScreen-Me/splitscreenme-nucleus"!** 

Otherwise, you will be attempting to merge your changes with a different repository. Double-check that you will be merging with the correct head branch as well (generally "master") Also verify that the _head_ repository is set to your fork and the branch of your fork where you made all the changes that you would like to implement.

It is a good idea to create a detailed (but concise) comment to accompany the initial pull request to explain:
* The issue the pull request is addressing
* What is changed
* Testing procedures used and their results
* Potential issues or challenges encountered
* Any questions or other comments for contribution reviewers

#### Review Process
All pull requests will need to be approved by a sufficient number of maintainers before any changes are merged. Before approving changes, reviewers will make sure that:
* Changes do not interfere with existing features or design plans
* There are no overlap issues (i.e. other developers are already assigned to it) 
* Sufficient testing occurred
* Quality standards are upheld

While it is certainly _possible_ that a quality pull request might be accepted right away, change requests, questions, and discussions should be expected before the pull request can be merged. Discussions around the pull request should preferably be held in the pull request comments. This way it is easy for multiple people to follow progress... but do not hesitate to reach out directly to the developers and maintainers either!


### Implementing Changes

#### Approval
Once the review process is completed to the satisfaction of enough maintainers, a maintainer will merge the pull request with the "master" branch, incorporating any changes within the pull request.

At this point, the contributor's job is done! If your pull request made it this far, congratulations! Your contribution is now part of the project!
Thank you for contributing!

#### Release Builds
Periodically, the current lead developer(s) of Nucleus Coop will decide when enough changes have been incorporated into the master branch to justify a new release. To ensure build stability, a release will be created and published alongside a change log of implemented changes after sufficient testing by maintainers. Releases include source code along with a more convenient installer and a "portable" (pre-compiled zip folder) option for users to run the program.

Currently, there is no set release schedule, and releases will be published when ready and verified stable.

### Code Standards
* When in doubt, structure your code to prioritize readability!
* Comment large blocks of abstract code with concise descriptions.
* Please write C# code following [standard conventions](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions). 

### Tips and Troubleshooting

#### Updating Submodules
If you get an error about missing URL's when attempting to initialize and update the submodules, open up .gitmodules (in the project root folder) in a text editor and make sure that the following are listed. 
    
Even if some of these dependencies are not in active use, the git "update" command may fail to execute if entries and URLs for **all submodule** dependencies cloned from your fork are not found in the .gitmodules file. Add anything missing, save the changes to .gitmodules and the command should now work.
```ini
[submodule "Submodules/x360ce"]
    path = Submodules/x360ce
    url = https://github.com/lucasassislar/x360ce

[submodule "Submodules/ProtoInput"]
    path = Submodules/ProtoInput
    url = https://github.com/ilyaki/protoinput

[submodule "Submodules/nukeupdater"]
    path = Submodules/nukeupdater
    url = https://github.com/lucasassislar/nukeupdater
    
[submodule "Submodules/TypescriptSyntaxPaste"]
    path = Submodules/TypescriptSyntaxPaste
    url = https://github.com/nhabuiduc/TypescriptSyntaxPaste.git
```

#### Issues Compiling Proto Input
Proto Input appears to be very sensitive to its development environment. If you are opening ProtoInput.sln in Visual Studio 2022, you may be prompted to upgrade the project, but doing this might complicate things. If you cannot get it to compile, try opening a fresh version of ProtoInput.sln (so you see the upgrade prompt again) and make sure that all options are set to "No Upgrade". Upgrading can potentially lead to syntax errors in the compiling process. If you see a `FILE_INFO_BY_HANDLE_CLASS` syntax error pop up numerous times during compiling, this is a likely culprit.

If you run into issues compiling ProtoInput.sln, there is a good chance that you are missing some dependencies or add-ons required by Visual Studio or Visual Studio Build Tools, and you will need to identify and install them. You may also refer to the [Compilation](https://nucleus-coop.github.io/docs/compilation/) article for a more detailed example (with screenshots) of creating builds from the source code. Troubleshooting may also involve fixing ProtoInput.sln project build order dependencies if sub-components appear to be building out of the intended order on your setup. In addition to basic C++ dependencies, you will also need to install C++ ATL for build tools (x86/64), at the minimum, and possibly v142 C++ dependencies.

You may also need to update the compiling order. In some dev environments, it appears that the default project compile order may result in certain subcomponents trying to compile before their dependent libraries are finished coding. Keep an eye on output errors from the compile process and note any errors that occur because a file cannot be opened. Make sure that the sub-component that failed to compile has the project that contains the file that failed to open as a dependency. To set dependencies, right-click the ProtoInput solution in the solution explorer section of Visual Studio and select the "Project Dependencies..." option. From there, simply use the drop-down box to select a subcomponent and then check the boxes next to any known, missing dependencies (as seen from output errors). Submodules with known issues in certain setups include _ProtoInputInjectorProxy_ (requires EasyHookDLL) and _ProtoInputUtilStatic_ (requires Blackbone). Check these submodules first and ensure they have their dependencies checked if you have any issues. Otherwise use output errors to help troubleshoot any other dependency issues. A file failing to open is a good indicator that it was not installed yet, which suggests it is a dependency that should have been installed earlier.

#### Project Branches
The "master" branch of the project is the primary development branch of Nucleus Coop. Generally, you should be safe to assume that this is the current branch for everyone to work with. If you ever see other branches with recent updates and aren't sure which branch you should be forking or working from, just ask!

**When you submit a Pull Request, make _sure_ that you have the correct merge target and source set.** The "base repository" is the target _destination_ for your changes, while the "head repository" is the _source_ of the changes (i.e. your forked repository).

GitHub may default the **base** to a _different_, earlier fork (such as ZeroFox5866's fork of the project). If this happens, you will need to do the following to avoid submitting your change to the wrong repository:

1) Set the **head** to your own fork (it may default to "SplitScreen-Me/splitscreenme-nucleus")
2) Set the **base** to "SplitScreen-Me/splitscreenme-nucleus"
3) Verify that the correct branches for the base and head are selected

**Caution** - There may be some delay or browser hitching while selecting repositories.

## Donations
If you would like to contribute financially to support the project, check out the links under the "Sponsor this Project" section of the [repository homepage](https://github.com/SplitScreen-Me/splitscreenme-nucleus). Donations are always appreciated, but completely voluntary!

# Other Resources
If you can't find the answers you are looking for here, you may want to take a look at some of the other project documentation.
* The [Official FAQ](https://www.splitscreen.me/docs/faq) answers a lot of common questions about the project!
* The [Documents](https://nucleus-coop.github.io/docs/quickstart/) section of the Nucleus Coop website is a good place to get started! It has helpful links to a lot of key setup resources. It also has detailed instructions for compiling Nucleus Coop and working with Proto Input.

If you still can't find an answer to your question, feel free to ask around on the support or developer channels of the [Nucleus Coop Discord Server](https://discord.gg/QDUt8HpCvr), depending on the nature of what you are looking for.
