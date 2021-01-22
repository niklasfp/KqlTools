const core = require('@actions/core');
const github = require('@actions/github');

const fs = require('fs');
const util = require('util');
const readdir = util.promisify(fs.readdir);

async function deleteReleaseAssets(octokit, owner, repo, oldAssets, newAssets) {
	var deleteSuccess = true;
	if(oldAssets != null)
	{
		for (let oldAsset of oldAssets)
		{
			for (let newAsset of newAssets)
			{				
				if(oldAsset.name == newAsset)
				{
					// delete old copy of release asset
					console.log(`Deleting old copy of ${newAsset}...`);

					const response = await octokit.repos.deleteReleaseAsset({
						owner: owner,
						repo: repo,
						asset_id: oldAsset.id
					});

					if(response.status != 204)
					{
						deleteSuccess = false;
					}

					console.log(`Deleted old copy of ${newAsset}.`);
					break;
				}
			}
		}
	}
	return deleteSuccess;
}

async function run() {
	try {
		// set up octokit and context information
		const octokit = github.getOctokit(core.getInput('token'));

		// getting context
		const owner = github.context.repo.owner;
		const repo = github.context.repo.repo;

		// get directory with assets to upload
		const dir = core.getInput('directory');

		// get latest release
		const getLatestResponse = await octokit.repos.getLatestRelease({
			owner: owner,
			repo: repo
		});

		// getting arrays of old and new assets
		const latestRelease = getLatestResponse.data;
		const oldAssets = latestRelease.assets;
		const newAssets = await readdir(dir);

		// deleting all clashing old assets first
		if(await deleteReleaseAssets(octokit, owner, repo, oldAssets, newAssets)) {
			// uploading new assets
			for (let newAsset of newAssets) {
				console.log(`Uploading ${newAsset}...`);

				const uploadResponse = await octokit.repos.uploadReleaseAsset({
					owner: owner,
					repo: repo,
					release_id: latestRelease.id,
					name: newAsset,
					data: fs.readFileSync(`${dir}\\${newAsset}`),
					origin: latestRelease.upload_url
				});

				console.log(`Uploaded ${newAsset}.`);
			}
		}
	}
	catch (err)
	{
		core.setFailed(err);
	}
}

run();