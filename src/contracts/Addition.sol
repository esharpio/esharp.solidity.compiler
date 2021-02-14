pragma solidity ^0.4.0;

contract Addition {
    uint storedData;

    function set(uint x) public {
        storedData = x;
    }

    function add(uint a, uint b) public view returns (uint) {
        return a + b;
    }
}