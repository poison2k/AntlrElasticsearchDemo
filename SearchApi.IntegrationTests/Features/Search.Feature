Feature: Search API
    Scenario: OR query returns hits
        Given the index "documents" exists
        And the following documents are indexed:
            | id | title | description | text             |
            | 1  | One   | Alpha       | apple banana     |
            | 2  | Two   | Beta        | banana  cherry   |
            | 3  | Three | Gamma       | stawberry cheese |
        When I search with query "apple OR cherry"
        Then the response status should be 200 
        And the result ids should contain:
        | id |
        | 1  |
        | 2  |