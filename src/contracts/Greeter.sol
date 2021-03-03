
pragma solidity ^0.4.2;

// Example taken from https://www.ethereum.org/greeter, also used in
// https://github.com/ethereum/go-ethereum/wiki/Contract-Tutorial#your-first-citizen-the-greeter

contract Greeter {
    /* define variable greeting of the type string */
    string greeting;

    /* this runs when the contract is executed */
    constructor(string _greeting) public {
        greeting = _greeting;
    }

    function newGreeting(string _greeting) public {
        greeting = _greeting;
    }

    /* main function */
    function greet() public view returns (string) {
        return greeting;
    }
}
