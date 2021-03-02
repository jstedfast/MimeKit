# !_PROJECT_NAME_! CI/CD documentation

* after merging the PR, the first run of the main workflow will not complete successfully, because it requires specific setup explained in this documentation

## *. Set up NuGet

NuGet is a package manager and is responsible for downloading, installing, updating, and configuring software in your system. From the term software we don't mean end users software like Microsoft Word or Notepad 2, etc. but pieces of software, which you want to use in the project, assembly references.

1. Go to https://www.nuget.org/ .

2. Log in or create your account.

3. From the dropdown in the right top corner (with your account name on it) choose **API Keys**.

4. Create a new token by clicking the "+ Create" button.

5. Set the **Key name** to "NUGET TOKEN" so it's compatible with your workflow.

6. On **Package Owner** choose your username from the dropdown.

7. To select which packages to associate with a key, use a glob pattern, select individual packages, or both.

8. Set your package name in the **Glob Pattern** field.

9. Now you have to set the token as a **secret** in GitHub in order to make it work.

### Creating encrypted secrets for a repository

1. On GitHub, navigate to the main page of the repository.

2. Under your repository name, click  Settings.

3. Repository settings button.

4. In the left sidebar, click Secrets.

5. Click New repository secret.

6. Type the name "NUGET_TOKEN" as a name of your secret.

7. Enter the value for your secret.

8. Click Add secret.
   **You are all set up**

<br>

## *. Run workflow manually

Once you've set up all the steps above correctly, you should be able to successfully complete a manual execution of the "!_PROJECT_NAME_! CI/CD Pipeline" workflow.

  1. Go to the project's GitHub repository and click on the **Actions** tab

  2. From the "Workflows" list on the left, click on "!_PROJECT_NAME_! CI/CD Pipeline"

  3. On the right, next to the "This workflow has a workflow_dispatch event trigger" label, click on the "Run workflow" dropdown, make sure the default branch is selected (if not manually changed, should be main or master) in the "Use workflow from" dropdown and click the "Run workflow" button

![Actions_workflow_dispatch](/ScreenShots/CI-CD_DOCUMENTATION/Actions_workflow_dispatch.png)

  4. Once the workflow run has completed successfully, move on to the next step of the documentation

NOTE: **screenshots are only exemplary**

<br>

## How to create a PAT

- In a new tab open GitHub, at the top right corner, click on your profile picture and click on **Settings** from the dropdown.

	![CSA_new_pat_1](/ScreenShots/CI-CD_DOCUMENTATION/CSA_new_pat_1.png)

- Go to Developer Settings -> Personal access tokens.

	![CSA_new_pat_2](/ScreenShots/CI-CD_DOCUMENTATION/CSA_new_pat_2.png)

	![CSA_new_pat_3](/ScreenShots/CI-CD_DOCUMENTATION/CSA_new_pat_3.png)

- Click the **Generate new token** button and enter password if prompted.

	![CSA_new_pat_4](/ScreenShots/CI-CD_DOCUMENTATION/CSA_new_pat_4.png)

- Name the token, from the permissions list choose the ones needed and at the bottom click on the **Generate token** button.

	![CSA_new_pat_5](/ScreenShots/CI-CD_DOCUMENTATION/CSA_new_pat_5.png)

- Copy the token value and paste it wherever its needed 

	![CSA_new_pat_6](/ScreenShots/CI-CD_DOCUMENTATION/CSA_new_pat_6.png)

NOTE: once you close or refresh the page, you won't be able to copy the value of the PAT again!

#

Built with ❤ by [Pipeline Foundation](https://pipeline.foundation)