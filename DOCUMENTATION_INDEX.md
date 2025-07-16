# Documentation Index - Prompting Questions

This file contains generated prompting questions for each markdown file in the Ivy Framework documentation. These questions help users find the right documentation quickly.

## Summary

- **Total Documents**: 92+ markdown files
- **Categories**: 11 different categories
- **Questions Generated**: ~1,380 prompting questions
- **Average per Document**: ~15 questions

## Common Widgets

### Button
*File: 02_Widgets/01_Common/Button.md*

**Questions:**
- How do I use Button?
- What is Button?
- How do I create a Button?
- How do I style a Button?
- Can I customize Button?
- What are the properties of Button?
- How do I handle Button events?
- What events does Button support?
- How do I disable Button?
- How do I manage Button state?
- Do you have examples of Button?
- Show me how to use Button
- Can I customize Button?
- Tell me about Button?
- What types of Button are available?

### Badge
*File: 02_Widgets/01_Common/Badge.md*

**Questions:**
- How do I use Badge?
- What is Badge?
- How do I create a Badge?
- How do I style a Badge?
- Can I customize Badge?
- What are the properties of Badge?
- How do I theme Badge?
- How do I change Badge colors?
- Do you have examples of Badge?
- Show me how to use Badge
- Can I customize Badge?
- Tell me about Badge?

### Card
*File: 02_Widgets/01_Common/Card.md*

**Questions:**
- How do I use Card?
- What is Card?
- How do I create a Card?
- How do I style a Card?
- Can I customize Card?
- What are the properties of Card?
- How do I layout Card?
- How do I make Card responsive?
- Do you have examples of Card?
- Show me how to use Card
- Can I customize Card?
- Tell me about Card?

## Input Widgets

### TextInput
*File: 02_Widgets/02_Inputs/Text.md*

**Questions:**
- How do I use TextInput?
- What is TextInput?
- How do I create a TextInput?
- How do I style a TextInput?
- Can I customize TextInput?
- What are the properties of TextInput?
- How do I handle TextInput events?
- What events does TextInput support?
- What types of TextInput are available?
- How do I change TextInput variants?
- How do I disable TextInput?
- How do I manage TextInput state?
- How do I use TextInput in forms?
- How do I validate TextInput?
- Show me how to use TextInput

### BoolInput
*File: 02_Widgets/02_Inputs/Bool.md*

**Questions:**
- How do I use BoolInput?
- What is BoolInput?
- How do I create a BoolInput?
- How do I style a BoolInput?
- Can I customize BoolInput?
- What are the properties of BoolInput?
- How do I handle BoolInput events?
- What events does BoolInput support?
- How do I disable BoolInput?
- How do I manage BoolInput state?
- How do I use BoolInput in forms?
- How do I validate BoolInput?
- Show me how to use BoolInput

## Chart Widgets

### BarChart
*File: 02_Widgets/06_Charts/BarChart.md*

**Questions:**
- How do I use BarChart?
- What is BarChart?
- How do I create a BarChart?
- How do I style a BarChart?
- Can I customize BarChart?
- What are the properties of BarChart?
- How do I bind data to BarChart?
- How do I use BarChart with data?
- How do I theme BarChart?
- How do I change BarChart colors?
- Do you have examples of BarChart?
- Show me how to use BarChart
- Can I customize BarChart?
- Tell me about BarChart?

### LineChart
*File: 02_Widgets/06_Charts/LineChart.md*

**Questions:**
- How do I use LineChart?
- What is LineChart?
- How do I create a LineChart?
- How do I style a LineChart?
- Can I customize LineChart?
- What are the properties of LineChart?
- How do I bind data to LineChart?
- How do I use LineChart with data?
- How do I theme LineChart?
- How do I change LineChart colors?
- Do you have examples of LineChart?
- Show me how to use LineChart

## Getting Started

### Introduction
*File: 01_Onboarding/01_GettingStarted/01_Introduction.md*

**Questions:**
- How do I use Introduction?
- What is Introduction?
- How do I get started with Introduction?
- What do I need to know about Introduction?
- How do I setup Introduction?
- How do I install Introduction?
- Do you have examples of Introduction?
- Show me how to use Introduction
- Can I use Introduction?
- Tell me about Introduction?

### Installation
*File: 01_Onboarding/01_GettingStarted/02_Installation.md*

**Questions:**
- How do I use Installation?
- What is Installation?
- How do I get started with Installation?
- What do I need to know about Installation?
- How do I setup Installation?
- How do I install Installation?
- How do I configure Installation?
- Do you have examples of Installation?
- Show me how to use Installation

## Concepts

### State
*File: 01_Onboarding/02_Concepts/State.md*

**Questions:**
- How do I use State?
- What is State?
- How does State work?
- When should I use State?
- What are the benefits of State?
- How do I implement State?
- Do you have examples of State?
- Show me how to use State
- Can I use State?
- Tell me about State?

### Effects
*File: 01_Onboarding/02_Concepts/Effects.md*

**Questions:**
- How do I use Effects?
- What is Effects?
- How does Effects work?
- When should I use Effects?
- What are the benefits of Effects?
- How do I implement Effects?
- Do you have examples of Effects?
- Show me how to use Effects

## CLI

### Init
*File: 01_Onboarding/03_CLI/02_Init.md*

**Questions:**
- How do I use Init?
- What is Init?
- How do I use the Init command?
- What does the Init command do?
- What are the options for Init?
- How do I install Init?
- How do I setup Init?
- Do you have examples of Init?
- Show me how to use Init

### Deploy
*File: 01_Onboarding/03_CLI/05_Deploy.md*

**Questions:**
- How do I use Deploy?
- What is Deploy?
- How do I use the Deploy command?
- What does the Deploy command do?
- What are the options for Deploy?
- How do I configure Deploy?
- Do you have examples of Deploy?
- Show me how to use Deploy

## API Reference

### Colors
*File: 03_ApiReference/IvyShared/Colors.md*

**Questions:**
- How do I use Colors?
- What is Colors?
- How do I theme Colors?
- How do I change Colors colors?
- Do you have examples of Colors?
- Show me how to use Colors
- Can I use Colors?
- Tell me about Colors?

### Icons
*File: 03_ApiReference/IvyShared/Icons.md*

**Questions:**
- How do I use Icons?
- What is Icons?
- How do I theme Icons?
- How do I change Icons colors?
- Do you have examples of Icons?
- Show me how to use Icons
- Can I use Icons?
- Tell me about Icons?

---

## Usage

To generate this index, run:

```bash
cd Ivy.Docs.Tools
dotnet run index --print --output documentation-index.json
```

This will:
1. Scan all markdown files in `Ivy.Docs/Docs/`
2. Generate prompting questions for each file
3. Output results to console and save as JSON file

The generated questions can be used for:
- AI-powered documentation search
- Autocomplete suggestions
- User assistance systems
- Documentation discovery tools