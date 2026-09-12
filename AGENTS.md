# Msmapwr's AGENTS.md

> 本文件是 Msmapwr 的项目的 Agent 开发规范。
>
> Agent 在修改项目、执行 Git 操作、设计功能、测试、发布版本以及处理 GitHub 工作流时，**MUST** 遵循本文件。

---

# 1. 总体原则

Agent 在修改项目时必须遵循以下优先级：

1. **正确性**
2. **安全性**
3. **可测试性**
4. **合理的任务拆解**
5. **代码质量**
6. **向后兼容性**
7. **清晰的 Git 历史**
8. **远程仓库状态明确**
9. **版本与变更记录完整**
10. **功能实现速度**

如果不同规则发生冲突：

> **安全性 > 正确性 > 测试 > 用户现有数据 / 修改 > 代码质量 > 开发速度**

如果“快速完成”和“正确完成”发生冲突，**MUST** 优先保证正确性。

如果“方便操作”和“保护用户已有修改”发生冲突，**MUST** 优先保护用户已有修改。

---

# 2. 项目开始前检查

每次开始处理一个项目或新的开发任务时，Agent **MUST** 先进行项目环境检查。

基本流程：

```text
开始任务
    ↓
读取 AGENTS.md
    ↓
检查项目目录
    ↓
检查 Git 状态
    ↓
检查当前 Branch
    ↓
检查 Git Remote
    ↓
检查项目技术栈
    ↓
检查 README / 项目文档
    ↓
检查依赖
    ↓
检查是否存在用户未提交修改
    ↓
判断更新规模
    ↓
进入对应开发流程
```

Agent **MUST** 在正式修改代码之前了解：

- 项目使用的主要技术栈
- 项目构建方式
- 项目测试方式
- 项目运行方式
- 项目目录结构
- 当前 Git 分支
- Git Remote
- 当前工作区状态
- 当前版本号
- `CHANGELOG.md`
- README 或其他项目文档

如果项目存在其他更高优先级的 Agent 配置文件，也必须遵循其规则。

---

# 3. Git 管理

## 3.1 GitHub 远程仓库

在开始一个新项目或首次接手一个尚未确认远程仓库的项目时，Agent **MUST** 向用户获取 GitHub Repository 地址。

例如：

```text
请提供该项目的 GitHub Repository 地址，以便配置 Git remote 并在完成开发后推送代码。
```

获取 GitHub 地址后，Agent **MUST**：

1. 检查项目是否已经初始化 Git。
2. 检查当前 Git Remote。
3. 如果没有 Remote，则使用用户提供的地址配置 `origin`。
4. 如果已经存在 Remote，则检查 Remote 地址。
5. 如果现有 Remote 与用户提供的地址不一致，**MUST NOT** 擅自覆盖。
6. 必须向用户说明差异并请求确认。
7. 确认后才能修改 Remote。

Agent **MUST NOT**：

- 猜测 GitHub Repository 地址。
- 虚构 GitHub Repository 地址。
- 未经用户确认修改已有 Remote。
- 将代码推送到未知 Repository。
- 将代码推送到未经用户确认的 Repository。
- 在没有远程地址时声称已经完成 Push。

如果用户暂时没有提供 GitHub 地址：

> Agent 可以进行本地开发和本地 Commit，但必须明确说明当前无法完成远程 Push。

---

## 3.2 Git 初始化

每个项目都必须使用 Git。

开始任务时 **MUST** 检查项目根目录是否存在 `.git`。

如果：

```text
.git 存在
```

则使用现有 Git 仓库。

如果：

```text
.git 不存在
```

则：

```text
git init
```

Agent **MUST NOT** 在已经存在 Git 仓库的项目中重复初始化 Git。

---

## 3.3 Git Status 检查

开始修改之前必须检查：

```text
git status
```

如果发现工作区存在修改，Agent **MUST** 判断这些修改是否属于当前任务。

如果无法确定：

> **MUST 向用户询问。**

---

## 3.4 保护用户已有修改

如果工作区存在不是 Agent 当前任务产生的修改：

Agent **MUST NOT**：

```text
git reset --hard
git clean -fd
git checkout .
git restore .
```

也不得：

- 删除用户修改
- 覆盖用户修改
- 擅自 Stash 用户修改
- 擅自 Reset 用户 Commit
- 擅自 Rebase 用户历史
- 擅自 Force Push

任何可能破坏用户现有工作的操作，都必须先获得用户明确允许。

---

# 4. Git Branch

项目应使用清晰的 Branch 管理。

推荐：

```text
main
│
├── feature/xxx
├── fix/xxx
├── refactor/xxx
└── chore/xxx
```

命名建议：

```text
feature/<name>
fix/<name>
refactor/<name>
docs/<name>
chore/<name>
perf/<name>
```

例如：

```text
feature/i18n
feature/multiplayer
fix/login-button
refactor/ui-system
```

对于简单的小更新，如果项目当前工作流直接使用 `main`，可以遵循项目已有流程。

对于大更新、超大更新或高风险修改：

> **SHOULD 使用独立 Branch。**

Agent **MUST NOT** 未经用户允许删除远程 Branch。

---

# 5. Git Commit

每完成一个**独立且可工作的最小功能单元**，并通过必要测试后，**MUST** 创建一次 Commit。

Commit Message **MUST** 遵循 AngularJS Git Commit Message Conventions：

```text
<type>(<scope>): <subject>
```

例如：

```text
feat(player): add movement system
fix(auth): handle expired token
refactor(ui): simplify button component
docs(readme): update installation guide
test(player): add collision tests
chore(deps): update dependencies
perf(render): optimize sprite rendering
```

常用类型：

- `feat`：新增功能
- `fix`：Bug 修复
- `docs`：文档
- `style`：代码格式
- `refactor`：重构
- `test`：测试
- `chore`：构建、依赖、工具
- `perf`：性能优化

要求：

- 每个 Commit **SHOULD** 只包含一个逻辑上的变更。
- Commit Message **MUST** 清晰表达变更内容。
- 禁止使用模糊的：

```text
update
modify
change
stuff
test
```

作为无法表达具体内容的 Commit Message。

不得为了凑 Commit 数量而制造无意义 Commit。

---

# 6. Commit 前检查

每次 Commit 前 **MUST** 检查：

```text
git status
```

并确认：

- 修改属于当前任务。
- 没有意外加入其他文件。
- 没有敏感信息。
- 测试已经通过。
- 不存在明显构建错误。
- Commit 内容与 Commit Message 一致。

---

# 7. Secrets 与敏感信息

Agent **MUST NOT** 将敏感信息提交到 Git。

包括但不限于：

```text
API Key
Access Token
Password
Private Key
Secret
Certificate
数据库密码
OAuth Secret
云服务凭据
.env
.env.*
```

除非项目明确要求，否则不得将真实 Secrets 加入 Repository。

应该使用：

```text
.env.example
```

或项目规定的配置模板。

如果发现敏感信息已经进入 Git：

1. **MUST 停止 Push。**
2. 告知用户。
3. 移除敏感信息。
4. 如果 Secret 已经进入历史，必须提醒用户该 Secret 可能已经泄露。
5. 必要时建议立即轮换 / 撤销 Secret。

**禁止为了完成任务而忽略 Secrets 风险。**

---

# 8. 更新规模定义

所有开发任务必须划分为：

1. **小更新（Small Update）**
2. **大更新（Major Update）**
3. **超大更新（Massive Update）**

Agent **MUST** 在开始实施前判断更新规模。

代码行数不是主要判断标准。

主要判断：

- 影响范围
- 功能完整度
- 模块数量
- 架构变化
- 技术栈变化
- UI / UX 变化
- 数据结构变化
- 核心业务变化
- 兼容性影响
- 是否需要多个开发阶段

---

# 9. 小更新

## 9.1 定义

小更新：

> 对项目进行局部、单一、低影响范围的修改，不改变项目整体结构、核心技术方向或主要功能体系。

典型实例：

- 修复 Bug
- 修改一个参数
- 删除一个方法
- 修改一个函数
- 修改一个配置项
- 修改一个组件
- 修改一个样式
- 增加简单校验
- 优化一处代码
- 增加简单测试
- 删除无用文件
- 更新一个依赖版本

例如：

```text
修复登录按钮无法点击
→ 小更新

修改游戏移动速度
→ 小更新

删除废弃方法
→ 小更新
```

---

## 9.2 小更新流程

```text
小更新
    ↓
实现
    ↓
必要测试
    ↓
安全检查
    ↓
Commit
    ↓
Push
```

---

# 10. 大更新

## 10.1 定义

大更新：

> 增加一个相对完整的新功能，或者对一个独立功能模块进行明显扩展，但不会从根本上改变整个项目的技术架构、设计方向或核心体系。

典型实例：

- 增加 i18n
- 网页新增页面
- 游戏增加联机功能
- 增加完整登录系统
- 增加数据库
- 增加游戏模式
- 增加排行榜
- 增加支付系统
- 增加后台模块
- 增加完整主题系统

---

## 10.2 大更新必须拆解

每一个大更新：

> **MUST 拆解为两个或多个小更新。**

小更新数量不限。

例如：

```text
大更新：增加 i18n

├── 小更新：建立语言资源结构
├── 小更新：增加语言切换
├── 小更新：国际化首页
├── 小更新：国际化 About 页面
├── 小更新：语言持久化
└── 小更新：增加 i18n 测试
```

Agent **MUST NOT** 将明显属于大更新的任务直接作为一个 Commit 完成。

---

# 11. 超大更新

## 11.1 定义

超大更新：

> 对整个项目进行大规模、系统性的改变，会显著影响多个模块、整体 UI、技术架构、视觉体系、交互方式或项目技术方向。

典型实例：

- 重构整个 UI
- 更换技术栈
- 游戏全面更新材质
- 全面扁平化设计
- 整体重新设计网站
- 前端框架迁移
- 大规模架构重构
- 全面重做游戏视觉系统
- 全面重做交互系统
- 同时修改多个核心系统
- 项目整体现代化改造

---

## 11.2 超大更新必须拆解

每一个超大更新：

> **MUST 拆解为两个或多个大更新。**

每个大更新：

> **MUST 再拆解为两个或多个小更新。**

完整结构：

```text
超大更新
│
├── 大更新
│   ├── 小更新
│   ├── 小更新
│   └── 小更新
│
├── 大更新
│   ├── 小更新
│   ├── 小更新
│   └── 小更新
│
└── 大更新
    ├── 小更新
    ├── 小更新
    └── 小更新
```

数量不限。

Agent 应根据实际复杂度拆解，不得为了满足数量机械拆分。

---

# 12. 更新规模判断原则

如果无法确定更新规模：

> **Agent 应选择更高一级的规模进行评估，而不是擅自按较低规模处理。**

例如：

```text
可能是小更新 / 大更新
→ 按大更新评估
```

```text
可能是大更新 / 超大更新
→ 按超大更新评估
```

这样可以避免低估任务复杂度。

---

# 13. 大更新需求确认

对于每一个大更新，在开始实际开发前：

> **MUST 至少向用户提出 6 个有效问题。**

6 个是最低数量，不是固定数量。

复杂任务可以询问更多。

---

## 13.1 问题覆盖范围

应尽可能覆盖：

1. 产品 / 功能目标
2. 功能范围
3. 用户体验
4. UI / UX
5. 技术方案
6. 项目结构
7. 数据结构
8. API
9. 兼容性
10. 性能
11. 安全性
12. 测试
13. 验收标准

---

## 13.2 禁止重复提问

用户已经明确说明的问题：

> **不得重复询问。**

问题数量是最低确认要求，不是机械提问要求。

例如用户已经明确：

```text
React + TypeScript
中文 / 英文
Tailwind
响应式
```

Agent 不得再次询问：

```text
使用什么技术栈？
支持什么语言？
要不要响应式？
```

应该只询问尚未明确且会影响开发的问题。

---

# 14. 超大更新需求确认

对于每一个超大更新：

> **MUST 至少向用户提出 12 个有效问题。**

12 个是最低数量。

复杂任务可以提出更多。

---

## 14.1 超大更新问题范围

至少覆盖：

### 产品

- 为什么进行这次更新？
- 最终目标是什么？
- 解决什么问题？
- 范围是什么？
- 哪些内容不在范围内？

### 用户体验

- 目标用户是谁？
- 希望提供什么体验？
- 是否存在参考产品？
- 是否存在 UI / UX 风格要求？
- 是否需要保持原有使用习惯？

### 技术

- 是否保留当前技术栈？
- 是否允许更换技术栈？
- 是否允许重构架构？
- 是否允许引入第三方库？
- 是否需要修改数据库 / API / 数据结构？

### 兼容性

- 是否兼容现有功能？
- 是否兼容旧数据？
- 是否需要迁移？
- 是否兼容旧客户端？
- 是否支持不同设备 / 浏览器 / 平台？

### 质量

- 是否存在性能指标？
- 是否存在安全要求？
- 是否需要自动化测试？
- 是否需要完整回归测试？
- 最终验收标准是什么？

---

# 15. 需求确认的特殊规则

超大更新开始时：

> **进行一次整体需求确认，至少 12 个问题。**

之后拆出的每个大更新：

> 如果已有信息足够明确，则**不需要重复询问 6 个问题**。

如果某个大更新存在新的、之前没有确定的关键问题：

> Agent **MUST** 进行针对性补充询问。

因此：

```text
超大更新
    ↓
≥12 个整体问题
    ↓
用户回答
    ↓
整体方案
    ↓
拆解大更新
    ↓
大更新存在额外不确定性？
    ├── 否 → 直接进入已确认方案
    └── 是 → 补充询问
```

---

# 16. 需求确认后不得直接开发

大更新 / 超大更新完成需求问答后，Agent **MUST**：

1. 整理用户回答。
2. 总结最终需求。
3. 明确功能范围。
4. 明确技术方案。
5. 明确兼容性要求。
6. 明确测试方案。
7. 明确 Acceptance Criteria。
8. 进行任务拆解。
9. 将计划展示给用户。
10. 获得用户确认后再开始实施。

---

# 17. Acceptance Criteria

每个大更新和超大更新都必须具有明确的验收标准。

例如：

```text
大更新：增加 i18n

验收标准：

- [ ] 支持中文
- [ ] 支持英文
- [ ] 可以切换语言
- [ ] 刷新后语言保持
- [ ] 所有页面不存在硬编码文本
- [ ] 不存在缺失翻译
- [ ] 自动化测试通过
- [ ] 回归测试通过
```

只有满足验收标准，才能认为更新完成。

---

# 18. 不确定性处理

如果用户没有回答某个问题：

### 关键问题

如果会影响：

- 技术栈
- 架构
- UI
- 数据结构
- API
- 安全
- 兼容性
- 最终结果

则：

> **MUST 继续询问用户。**

### 非关键问题

可以使用合理默认方案。

但必须明确：

```text
该项用户未指定，Agent 使用项目现有规范作为默认方案。
```

Agent **MUST NOT** 将自己的猜测描述成用户需求。

---

# 19. 依赖管理

增加、删除或升级依赖时：

Agent **MUST**：

1. 检查当前依赖。
2. 判断新依赖是否必要。
3. 优先使用项目已有依赖。
4. 检查兼容性。
5. 更新依赖配置。
6. 更新 Lockfile。
7. 运行构建。
8. 运行相关测试。

不得为了完成一个简单功能无意义地增加大型依赖。

---

# 20. 数据库与数据迁移

如果修改：

- 数据库结构
- 数据模型
- Schema
- API 数据结构
- 持久化格式

Agent **MUST** 考虑：

- 旧数据是否兼容
- 是否需要 Migration
- Migration 是否可逆
- 是否会造成数据丢失
- 是否需要备份
- 新旧版本是否能够共存

如果存在潜在数据破坏：

> **MUST 在执行前明确告知用户。**

禁止未经确认执行不可逆的数据破坏操作。

---

# 21. API 与兼容性

修改公共 API、接口、数据结构或外部调用方式时：

Agent **MUST** 判断是否属于 Breaking Change。

如果存在 Breaking Change：

- 必须明确记录。
- 必须考虑 Semantic Versioning。
- 必须更新 CHANGELOG。
- 必要时更新文档。
- 必要时提供迁移方案。

---

# 22. 测试要求

## 22.1 小更新

每个小更新完成后：

> **MUST 进行最小必要验证。**

包括适用的：

- 编译
- 构建
- 单元测试
- 类型检查
- Lint
- 启动
- 手动验证

测试通过后才能 Commit。

---

## 22.2 大更新

完成全部小更新后：

> **MUST 进行完整测试。**

至少：

- Build
- 自动化测试
- 回归测试
- 必要手动测试

---

## 22.3 超大更新

完成全部大更新后：

> **MUST 对整个项目进行最终完整测试。**

包括适用的：

- 完整 Build
- 全量测试
- 全量回归
- 核心功能
- UI / UX
- 性能
- 兼容性
- 安全性

---

# 23. 测试失败处理

测试失败时：

```text
测试失败
    ↓
定位问题
    ↓
判断代码问题 / 测试问题 / 环境问题
    ↓
修复
    ↓
重新测试
```

Agent **MUST NOT**：

- 删除失败测试
- 降低测试标准
- 修改预期结果来强行通过
- 绕过测试
- 隐藏错误
- 声称测试通过

如果确认测试本身已经不符合新的正确需求，可以修改测试，但：

> 必须确保新的测试符合实际需求，并明确这是需求变化，而不是为了消除失败。

---

# 24. Bug 修复

发现 Bug 后：

1. 定位原因。
2. 修复。
3. 增加 / 更新回归测试。
4. 运行测试。
5. 确认没有产生新问题。
6. 创建 `fix` Commit。
7. Push。

例如：

```text
fix(player): prevent movement through walls
fix(auth): handle expired access token
```

Bug 未验证修复成功前：

> **MUST NOT 声称修复完成。**

---

# 25. 失败恢复

如果 Agent 无法解决问题：

```text
停止继续扩大修改范围
        ↓
保留当前状态
        ↓
检查 Git 状态
        ↓
记录已尝试方案
        ↓
向用户报告
```

报告至少包含：

- 当前问题
- 错误原因
- 已尝试方案
- 当前项目状态
- 测试结果
- 下一步建议

Agent **MUST NOT**：

- 隐藏问题
- 删除问题代码以假装完成
- 回滚用户已有修改
- 为了“看起来正常”而破坏功能

---

# 26. 性能要求

如果更新可能影响性能，Agent **SHOULD**：

- 建立修改前基准
- 修改后进行比较
- 找出明显性能回归
- 优化高影响部分

对于游戏、实时应用、大数据处理、网络服务等项目：

> 性能属于重要验收指标。

---

# 27. UI / UX 更新

如果任务涉及 UI：

Agent 应考虑：

- Desktop
- Mobile
- Tablet
- 响应式布局
- 可访问性
- 交互反馈
- Loading
- Error
- Empty State
- Hover / Focus / Active
- Dark / Light Mode（如果项目支持）

大规模 UI 改造必须按照：

```text
Design System
    ↓
Layout
    ↓
Navigation
    ↓
Components
    ↓
Pages
    ↓
Responsive
    ↓
Migration
```

拆解。

---

# 28. 文档要求

如果功能改变了用户使用方式、安装方式、API、配置方式或开发方式：

Agent **MUST** 更新相关文档。

包括：

- README
- API 文档
- 使用文档
- 开发文档
- 配置示例
- Migration 文档

文档修改应使用：

```text
docs(...)
```

Commit。

---

# 29. CHANGELOG

项目 **MUST** 使用：

```text
CHANGELOG.md
```

并遵循 Keep a Changelog。

基本结构：

```markdown
# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security
```

开发过程中：

> 修改应优先记录到 `Unreleased`。

---

# 30. Semantic Versioning

项目版本号 **MUST** 遵循：

```text
MAJOR.MINOR.PATCH
```

规则：

```text
MAJOR
不兼容变更

MINOR
向后兼容的新功能

PATCH
向后兼容的 Bug 修复
```

例如：

```text
1.0.0 → 1.1.0
```

新增向后兼容功能。

```text
1.1.0 → 1.1.1
```

Bug 修复。

```text
1.1.1 → 2.0.0
```

Breaking Change。

Agent **MUST NOT** 无理由修改版本号。

---

# 31. CHANGELOG 与版本发布

开发期间不需要每个 Commit 都创建正式版本。

推荐：

```text
开发
 ↓
Unreleased
 ↓
完成更新
 ↓
测试
 ↓
准备发布
 ↓
确定版本
 ↓
CHANGELOG 归档
 ↓
Release Commit
 ↓
Git Tag
 ↓
Push
```

例如：

```markdown
## [1.2.0] - 2026-08-31

### Added

- Added internationalization.
- Added English language support.

### Fixed

- Fixed language persistence.
```

---

# 32. Release Commit

发布版本时可以使用：

```text
chore(release): prepare v1.2.0
```

然后：

```text
git tag v1.2.0
git push origin v1.2.0
```

Release Tag 必须遵循：

```text
vMAJOR.MINOR.PATCH
```

例如：

```text
v1.0.0
v1.2.0
v2.0.0
```

---

# 33. Pull Request

对于使用 Pull Request 工作流的项目：

大更新和超大更新：

> **SHOULD 使用 Pull Request。**

PR 应包含：

- 修改目标
- 修改范围
- 技术方案
- 测试结果
- Breaking Changes
- Migration
- 截图 / Demo（UI 项目适用）

PR 标题应遵循 Commit Message 风格。

例如：

```text
feat(i18n): add multilingual support
feat(multiplayer): add online multiplayer
refactor(ui): redesign application interface
```

---

# 34. CI

如果项目已经配置 CI：

Agent **MUST** 确保本地修改尽可能通过 CI 要求。

常见 CI 检查：

```text
Build
Test
Lint
Type Check
Security Check
```

如果 CI 失败：

```text
CI Failure
    ↓
定位原因
    ↓
修复
    ↓
Commit
    ↓
Push
    ↓
重新检查
```

不得忽略 CI Failure。

---

# 35. Issue / Task 关联

如果项目使用 GitHub Issues / Projects：

大更新和超大更新：

> **SHOULD 与对应 Issue / Task 关联。**

Commit / PR 可以关联：

```text
feat(i18n): add language switcher
```

PR 描述中应明确对应任务。

如果用户提供 Issue 编号：

> Agent 应在 Commit / PR 中合理关联。

---

# 36. 小更新、大更新、超大更新与 Git 的关系

## 小更新

```text
小更新
 ↓
测试
 ↓
Commit
 ↓
Push
```

---

## 大更新

```text
大更新
 ↓
≥6 个需求确认问题
 ↓
用户回答
 ↓
整理方案
 ↓
拆解多个小更新
 ↓
用户确认
 ↓
小更新
 ↓
测试
 ↓
Commit
 ↓
Push
 ↓
……
 ↓
所有小更新完成
 ↓
大更新完整测试
 ↓
Bug 修复
 ↓
最终测试
```

---

## 超大更新

```text
超大更新
 ↓
≥12 个需求确认问题
 ↓
用户回答
 ↓
整体方案
 ↓
拆解多个大更新
 ↓
每个大更新拆解多个小更新
 ↓
用户确认
 ↓
执行大更新
 ↓
大更新完整测试
 ↓
下一个大更新
 ↓
……
 ↓
所有大更新完成
 ↓
超大更新最终测试
 ↓
完整回归
 ↓
Bug 修复
 ↓
最终测试
 ↓
Release
```

---

# 37. Git Push

正常开发：

```text
小更新
 ↓
测试
 ↓
Commit
 ↓
Push
```

Push 前必须确认：

- Remote 正确
- Branch 正确
- 没有 Secrets
- Commit 内容正确
- 测试通过

Agent **MUST NOT** 未经用户允许：

```text
git push --force
git push --force-with-lease
```

除非用户明确要求或项目既有自动化流程明确要求。

---

# 38. Git History

Git History 应保持：

- 清晰
- 可读
- 可追踪
- 与实际开发过程一致

禁止：

- 无意义 Commit
- 大量 `update`
- 大量 `fix`
- 无意义 Merge
- 隐藏实际变更
- 为了好看随意重写历史

已经 Push 到远程的历史：

> **SHOULD NOT 随意重写。**

---

# 39. 大更新完成标准

大更新必须满足：

- [ ] 所有小更新完成
- [ ] 所有小更新均有合理 Commit
- [ ] 必要测试通过
- [ ] 完整 Build 通过
- [ ] 回归测试通过
- [ ] 已知 Bug 已处理
- [ ] Acceptance Criteria 全部满足
- [ ] 必要文档已更新
- [ ] CHANGELOG 已更新到 Unreleased
- [ ] Git History 清晰
- [ ] Remote 状态正确
- [ ] 必要 Commit 已 Push
- [ ] CI 通过（如果项目存在 CI）

---

# 40. 超大更新完成标准

超大更新必须满足：

- [ ] 已拆解为多个大更新
- [ ] 每个大更新已拆解为多个小更新
- [ ] 所有小更新完成
- [ ] 所有大更新完成
- [ ] 所有 Commit 清晰
- [ ] 完整 Build 通过
- [ ] 全量测试通过
- [ ] 全量回归通过
- [ ] 核心功能通过
- [ ] UI / UX 验证完成
- [ ] 性能验证完成（适用时）
- [ ] 兼容性验证完成
- [ ] 安全检查完成
- [ ] 已知 Bug 已处理
- [ ] Acceptance Criteria 全部满足
- [ ] 文档已更新
- [ ] CHANGELOG 已更新
- [ ] Semantic Versioning 已正确处理
- [ ] Git History 清晰
- [ ] CI 通过
- [ ] 所有必要 Commit 已 Push
- [ ] 最终状态与 GitHub Remote 同步

---

# 41. Release 完成标准

发布版本必须：

- [ ] 所有功能完成
- [ ] 所有测试通过
- [ ] CHANGELOG 完整
- [ ] Semantic Versioning 正确
- [ ] 版本号已更新
- [ ] Release Commit 已创建
- [ ] Git Tag 已创建
- [ ] Git Tag 已 Push
- [ ] Remote 状态同步
- [ ] CI 通过
- [ ] Release Notes 已准备（适用时）

---

# 42. Agent 最终执行原则

Agent 在任何情况下都必须遵循：

```text
理解需求
    ↓
判断更新规模
    ↓
必要的需求确认
    ↓
制定方案
    ↓
任务拆解
    ↓
用户确认
    ↓
开发
    ↓
测试
    ↓
Commit
    ↓
Push
    ↓
继续下一个更新
    ↓
完整测试
    ↓
Release
```

---

# 43. 严格禁止事项

Agent **MUST NOT**：

- 未测试就声称完成。
- 隐瞒测试失败。
- 隐瞒构建失败。
- 隐瞒已知 Bug。
- 为通过测试而删除测试。
- 为通过测试而绕过测试。
- 为通过测试而修改错误预期。
- 覆盖用户已有修改。
- 删除用户已有修改。
- 擅自 Reset。
- 擅自 Force Push。
- 擅自修改 Remote。
- 猜测 GitHub 地址。
- 推送到未知 Repository。
- 提交 Secrets。
- 无理由修改版本号。
- 忘记 CHANGELOG。
- 将大更新作为单个 Commit。
- 将超大更新作为单个或少数几个 Commit。
- 跳过任务拆解。
- 跳过大更新需求确认。
- 跳过超大更新需求确认。
- 为了凑问题数量重复提问。
- 将 Agent 自己的猜测描述成用户需求。
- 为了“看起来完成”而隐藏错误。
- 未经确认执行不可逆数据操作。
- 未经确认执行高风险破坏性 Git 操作。
- 随意重写已经 Push 的 Git 历史。

---

# 44. 核心规则速查

```text
┌──────────────────────────────────────────────┐
│              Msmapwr Agent Workflow          │
└──────────────────────────────────────────────┘

项目开始
   ↓
获取 GitHub Repository
   ↓
检查 Git
   ↓
检查 Remote
   ↓
检查 git status
   ↓
保护用户已有修改
   ↓
判断更新规模
   │
   ├── 小更新
   │      ↓
   │   直接实施
   │      ↓
   │   测试
   │      ↓
   │   Commit
   │      ↓
   │   Push
   │
   ├── 大更新
   │      ↓
   │   ≥6 个有效问题
   │      ↓
   │   用户回答
   │      ↓
   │   制定方案
   │      ↓
   │   拆解多个小更新
   │      ↓
   │   用户确认
   │      ↓
   │   小更新 → 测试 → Commit → Push
   │      ↓
   │   大更新完整测试
   │
   └── 超大更新
          ↓
       ≥12 个有效问题
          ↓
       用户回答
          ↓
       制定整体方案
          ↓
       拆解多个大更新
          ↓
       每个大更新拆解多个小更新
          ↓
       用户确认
          ↓
       执行全部大更新
          ↓
       每个大更新完整测试
          ↓
       超大更新最终测试
          ↓
       全量回归
          ↓
       Release
```

---

# 45. 最终工作模型

Msmapwr 项目的 Agent 开发模型：

```text
                    超大更新
                       │
             ┌─────────┼─────────┐
             ↓         ↓         ↓
           大更新     大更新     大更新
             │         │         │
          ┌──┼──┐   ┌──┼──┐   ┌──┼──┐
          ↓  ↓  ↓   ↓  ↓  ↓   ↓  ↓  ↓
         小  小  小 小  小  小 小  小  小
          │  │  │   │  │  │   │  │  │
          ↓  ↓  ↓   ↓  ↓  ↓   ↓  ↓  ↓
        Test Test Test Test Test Test Test
          │  │  │   │  │  │   │  │  │
          ↓  ↓  ↓   ↓  ↓  ↓   ↓  ↓  ↓
       Commit Commit Commit Commit Commit
          │  │  │   │  │  │   │  │  │
          ↓  ↓  ↓   ↓  ↓  ↓   ↓  ↓  ↓
        Push Push Push Push Push Push Push

                    ↓

              大更新完整测试

                    ↓

             超大更新完整测试

                    ↓

              CHANGELOG

                    ↓

             Semantic Version

                    ↓

              Release Commit

                    ↓

                 Git Tag

                    ↓

                  Push
```

**核心原则只有一句话：**

> **先理解，再确认；先拆解，再开发；先测试，再 Commit；先 Commit，再 Push；先完整验证，再 Release。**