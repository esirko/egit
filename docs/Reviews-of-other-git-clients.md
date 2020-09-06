
# Survey of existing clients for Mac OS and how they perform against my criteria

## Fork

The UI seems nice and reasonably snappy overall. You can have different tabs open for different repos; I could use that feature. The file browser looks good and has an "External Diff" option as well as a reasonable-looking internal diff solution that supports side-by-side.

There seems to be some advanced find functions that might come in handy. It says that it supports partial stashes, interesting feature. Saving as a patch appears to be supported.

But... I feel like I keep having to select the "First parent only" and "Hide remote branches" options. Shouldn't these options be saved?

I don't think Fork has the capability to diff the uncommitted changes with a commit a few changes down (Criterion #5). So that might be a dealbreaker.

## SourceTree

One nice thing I like is that the uncommitted changes appear at the top of the commit list view, like egit. In the file view, there are clear modified/deleted/added icons. Selecting two commits gives you all the files and you can create a patch from them! SourceTree seems to have a lot more options in the menubar than Fork.

The internal diff view seems to only support inline diffs, but I can set up BeyondCompare as an external diff tool.

SourceTree hits the most of my criteria out of all the options that I tested. If egit didn't exist, I would probably use SourceTree.

But... it doesn't have the file tree browser (criterion #9).

## Tower

Not free.

I'm having trouble just seeing the commit list view of the current HEAD branch. I still don't know how to get to just the HEAD branch without searching for it in the list. Even if there's a way, I haven't found it in a few minutes of looking, so the UI is not the clearest.

When you click two commits, it doesn't do the normal thing where it shows you the files that are different between those two commits. In fact, it shows you a screen with only two options: Revert and Diff, and clicking Diff does the really annoying thing of launching the external diff tool serially for each file. It doesn't tell you even how many files it's going to loop through before doing that.

They "integrated" the file list view and the diff view, so that when you select a file it shows the diff underneath. I don't really like that and it seems a bit more difficult to navigate with just a keyboard.

## SmartGit

They call the commit list view the "journal."

Looks like it does "first parent only" in the commit list view by default. Well wait. In the default view window, selecting a commit doesn't populate the UI in the way I think it should. Maybe they have a reason for that, but it's not an intuitive UI. And then there's a separate "log" window which looks very much the same except the journal is replaced by a graph (now not doing the "first parent only" thing) - so what's going on? Seems like some pretty confusing UI design.

OK, I'm revisiting SmartGit on the next day. I actually am _not_ seeing the "log" window at all, I'm seeing the "graph" which is what I want.  And there is an option to do "first parent only" so I selected that. So that's OK.

I can select BeyondCompare as the external diff tool. So far it looks pretty good.

But... When selecting two commits, with the first being the Working Tree, it doesn't actually show you the diffs between the uncommitted files and the selected second commit! And trying to edit the file will just put those changes in a temp folder!

## GitKraken

Hm, when opening for the first time you have to sign into either github or gitkraken. What if I want to just browse a local git repo? Not a good first impression. I tried opening a repo but it still wants me to sign in first.

I was really turned off by needing to sign in to do anything.

## gitk

I didn't realize this was already installed. Just type `gitk` from the commandline. From here, there's an option to "Start Git Gui"... hm, that opens up a new window with what looks like a new app. Who's in charge here?

From Git Gui, you can "visualize branch history" and that opens up a window that looks the same as the first window. So I guess that's the "gitk/Git Gui" commit history browser.

In any case, the commit list view doesn't observe "first parent only" mode and I can't figure out how to adjust that. I tried right-clicking on some of the commits in this window and was presented with a right-click menu in which some of the options were clearly not selectable but they weren't grayed out.  What a confusing user experience.