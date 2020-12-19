# CovidModel
A simulation that models the COVID infection rate
x : -17 to 17
z : -7 to 7
# TODO
- [x] Environment
- [x] 50 Agents
- [ ] agent code
  - [x] tasklist/location list
  - [x] on trigger enter
    - [x] not all trigger enters --> infection, calculate a percent that does.
    - [ ] infection from particles in room
    - [ ] infection rate increases based on proximity to sick? maybe
  - [ ] model room particles
  - [ ] updating trigger radius based on distancing measure
  - [x] change color for infection
  - [x] time limit for infection
  - [x] cured agent tag and material
  - [x] get room function so cant get infected through wall
  - [ ] outside vs inside infection chance
- [ ] simulation
  - [ ] variables
  - [ ] initializing agents
    - [x] initializing a task list
    - how to decide willDie?
	- 425k cases --> 18k deaths = 4% death rate
    - how to decide infectionTime
    - how long will a single simulation run for?
    - collecting data
- [ ] interacting with system?
  - UI buttons for variables
  - UI  show results
  - display results
- [ ] 2-4 minute video demo
