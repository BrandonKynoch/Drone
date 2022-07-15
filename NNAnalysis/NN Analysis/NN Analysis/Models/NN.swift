//
//  NN.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/14/22.
//

import Foundation

class NN {
    public let name: String
    
    private(set) var neuralSize: Int = 0
    private(set) var neuralShape = [Int]()
    private(set) var activations = [Int]()
    private(set) var layers = [NNLayer]()
    
    init(fromFile: URL) throws {
        name = fromFile.lastPathComponent
        
        let file = try FileHandle(forReadingFrom: fromFile)
        
        let fileData: Data = try file.readToEnd()!

        fileData.withUnsafeBytes({ pointer in
            var index = 0

            self.neuralSize = Int(pointer.ReadBytes(Int32.self, offset: &index))
            
            for _ in 0..<neuralSize {
                neuralShape.append(Int(pointer.ReadBytes(Int32.self, offset: &index)))
            }
            
            for _ in 0..<(neuralSize - 1) {
                activations.append(Int(pointer.ReadBytes(Int32.self, offset: &index)))
            }
            
            for i in 0..<(neuralSize - 1) {
                layers.append(NNLayer(
                    pointer: pointer,
                    shapeLeft: neuralShape[i],
                    shapeRight: neuralShape[i+1],
                    index: &index)
                )
            }
        })
    }
    
    
    
    struct NNLayer {
        var weights = [[Double]]()
        var biases = [Double]()
        
        init(pointer: UnsafeRawBufferPointer, shapeLeft: Int, shapeRight: Int, index: inout Int) {
            // shapeLeft = number of columns
            // shapeRight = number of rows
            for _ in 0..<shapeLeft {
                weights.append([Double]())
            }
            
            // Using row major order
            for i in 0..<shapeLeft { // Columns
                for j in 0..<shapeRight { // Rows
                    weights[i].append(pointer.ReadBytes(Double.self, offset: &index))
                }
            }
            
            for _ in 0..<shapeRight {
                biases.append(pointer.ReadBytes(Double.self, offset: &index))
            }
        }
    }
}
