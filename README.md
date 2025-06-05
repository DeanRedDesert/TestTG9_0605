# Studio Midas Game Template
This is the template game for all APAC studios to create games from.
 
## Create a new game from this template

Follow this checklist here or the for more details view [Creating a game](https://igt-ausstudio.atlassian.net/l/cp/0vetVH51). 

- [Create game repository from this template](https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-repository-from-a-template#creating-a-repository-from-a-template)
  * Make sure the owner of the new repository is `igt-ApacStudios`
  * Adhere to the naming convention `studioname-game-YourGameName`. eg `midas-game-LuckyGongMysticalEmperor`

- [Assign Studio team](https://docs.github.com/en/enterprise-cloud@latest/repositories/managing-your-repositorys-settings-and-features/managing-repository-settings/managing-teams-and-people-with-access-to-your-repository#changing-permissions-for-a-team-or-person) "Write" permissions
  * Assign your studio team with at least write permissions. eg `StudioMidas`, `StudioSpark`, `GLO Studio`
  * Without these permissions, no one else but you will be able to make changes

- [Clone the new game using GitHub Desktop](https://docs.github.com/en/desktop/contributing-and-collaborating-using-github-desktop/adding-and-cloning-repositories/cloning-a-repository-from-github-to-github-desktop)

- Create a submodule for the logic repo
  *  Run `.\Scripts\ChangeLogicRepo.cmd` and enter the address for the logic repo (eg `https://github.com/igt-ApacStudios/midas-support-logic-default`)
  *  Run `.\Scripts\UpdateLogic.cmd` to generate and copy the logic into the game.
  *  Push to Git.

- Update this [README](./README.md) to reflect the new game name and its description and remove all the above information.
