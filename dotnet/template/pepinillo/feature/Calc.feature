Feature: Calculator

  Background: 
    Given I have a calculator

  Scenario Outline: Add two numbers
    Given I have 2 numbers <a> and <b> as input
    When I add these 2 numbers
    And I add more numbers
      | number |
      |      1 |
      |      2 |
    Then I should get an output of <c>

    Examples: 
      | a | b | c |
      | 1 | 2 | 3 |
      | 2 | 3 | 5 |
