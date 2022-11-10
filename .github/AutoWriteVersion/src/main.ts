// 读取 package.json
import pkg from "../../../package.json" assert { type: "json" };

// 读取 version.txt
let version = await Deno.readTextFile("../version.txt");
if (version === pkg.version) {
  console.log("版本号一致，无需更新");
  Deno.exit(0);
}
const oldVersion = version;

// 写入 version.txt
version = pkg.version;
await Deno.writeTextFile("../version.txt", version);

// 替换 replaceList 中的版本号
const replaceList = [
  "../../../README.md"
]

for (const file of replaceList) {
  let readme = await Deno.readTextFile(file);
  readme = readme.replaceAll(oldVersion, version);
  await Deno.writeTextFile(file, readme);
}

// 创建 needUpdate.txt
// await Deno.writeTextFile("../needUpdate.txt", version);
