//
//  ContentView.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 2022/07/10.
//

import SwiftUI

let S_COL_MAIN: Color = Color(nsColor: NSColor(hex: "#003C34")!)
let S_COL_ACC1: Color = Color(nsColor: NSColor(hex: "#007269")!)
let S_COL_ACC2: Color = Color(nsColor: NSColor(hex: "#6C9778")!)
let S_COL_BACKGROUND: Color = Color(nsColor: NSColor(hex: "#DCCA8D")!)
let S_COL_BACKGROUND2: Color = Color(nsColor: NSColor(hex: "#FFD8A9")!)

let S_COL_CANCEL: Color = Color(nsColor: NSColor(hex: "#DE1D12")!)
let S_COL_APPROVE: Color = Color(nsColor: NSColor(hex: "#49DE1D")!)

let S_SHADOW_COL: Color = Color.black.opacity(0.25)

let S_CORNER_RADIUS: CGFloat = 12

struct MainView: View {
    var body: some View {
        ZStack {
            BackgroundView()
            
            VStack {
                HStack {
                    Text("Neural Network Analysis")
                        .modifier(TextModifier(size: 25, weight: .bold))
                        .foregroundColor(S_COL_MAIN)
                    Spacer()
                }
                .padding(.bottom)
                
                HStack {
                    Text("Select training folder")
                        .modifier(TextModifier(size: 15, weight: .light))
                        .foregroundColor(S_COL_MAIN)
                    Spacer()
                }
                
                Spacer()
            }
            .padding()
        }
    }
}

struct MainView_Previews: PreviewProvider {
    static var previews: some View {
        MainView()
    }
}
